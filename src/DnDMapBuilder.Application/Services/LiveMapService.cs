using System.Collections.Concurrent;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Events;
using DnDMapBuilder.Contracts.Interfaces;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PublicationStatusEntity = DnDMapBuilder.Data.Entities.PublicationStatus;
using PublicationStatusDto = DnDMapBuilder.Contracts.DTOs.PublicationStatus;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for managing real-time game map broadcasts and live view state.
/// Handles broadcasting map updates to SignalR clients and managing map publication status.
/// </summary>
public class LiveMapService : ILiveMapService
{
    private readonly IGameMapRepository _mapRepository;
    private readonly IGameMapHub _hubContext;
    private readonly IGameMapService _gameMapService;
    private readonly ILogger<LiveMapService> _logger;
    private readonly int _throttleWindowMs;
    private readonly ConcurrentDictionary<string, (DateTime LastBroadcast, SemaphoreSlim Lock)> _throttleState;

    public LiveMapService(
        IGameMapRepository mapRepository,
        IGameMapHub hubContext,
        IGameMapService gameMapService,
        ILogger<LiveMapService> logger,
        IConfiguration configuration)
    {
        _mapRepository = mapRepository;
        _hubContext = hubContext;
        _gameMapService = gameMapService;
        _logger = logger;
        _throttleWindowMs = int.TryParse(configuration["LiveMap:ThrottleWindowMs"], out var throttleMs) ? throttleMs : 100;
        _throttleState = new ConcurrentDictionary<string, (DateTime, SemaphoreSlim)>();
    }

    public async Task BroadcastMapUpdateAsync(string mapId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
        if (map == null)
        {
            _logger.LogWarning("Attempted to broadcast update for non-existent map {MapId}", mapId);
            return;
        }

        if (map.PublicationStatus != PublicationStatusEntity.Live)
        {
            _logger.LogDebug("Skipping broadcast for Draft map {MapId}", mapId);
            return;
        }

        var evt = new MapUpdatedEvent(
            map.Id,
            map.Name,
            map.Rows,
            map.Cols,
            map.GridColor,
            map.GridOpacity,
            map.ImageUrl,
            DateTime.UtcNow
        );

        var groupName = GetMapGroupName(mapId);
        await _hubContext.SendAsync(groupName, "MapUpdated", evt, cancellationToken);
        _logger.LogInformation("Broadcast map update for map {MapId}", mapId);
    }

    public async Task BroadcastTokenMovedAsync(string mapId, string tokenInstanceId, int x, int y, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
        if (map == null || map.PublicationStatus != PublicationStatusEntity.Live)
        {
            _logger.LogDebug("Skipping token moved broadcast for map {MapId}", mapId);
            return;
        }

        // Apply throttling for token movement broadcasts
        if (!await ShouldBroadcastAsync(mapId))
        {
            _logger.LogDebug("Token moved broadcast throttled for map {MapId}", mapId);
            return;
        }

        var evt = new TokenMovedEvent(mapId, tokenInstanceId, x, y, DateTime.UtcNow);
        var groupName = GetMapGroupName(mapId);
        await _hubContext.SendAsync(groupName, "TokenMoved", evt, cancellationToken);
        _logger.LogInformation("Broadcast token movement for token {TokenId} on map {MapId}", tokenInstanceId, mapId);
    }

    public async Task BroadcastTokenAddedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
        if (map == null || map.PublicationStatus != PublicationStatusEntity.Live)
        {
            _logger.LogDebug("Skipping token added broadcast for map {MapId}", mapId);
            return;
        }

        var tokenInstance = map.Tokens.FirstOrDefault(t => t.Id == tokenInstanceId);
        if (tokenInstance == null)
        {
            _logger.LogWarning("Token instance {TokenId} not found on map {MapId}", tokenInstanceId, mapId);
            return;
        }

        var evt = new TokenAddedEvent(mapId, tokenInstance.Id, tokenInstance.TokenId, tokenInstance.X, tokenInstance.Y, DateTime.UtcNow);
        var groupName = GetMapGroupName(mapId);
        await _hubContext.SendAsync(groupName, "TokenAdded", evt, cancellationToken);
        _logger.LogInformation("Broadcast token addition for token {TokenId} on map {MapId}", tokenInstanceId, mapId);
    }

    public async Task BroadcastTokenRemovedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
        if (map == null || map.PublicationStatus != PublicationStatusEntity.Live)
        {
            _logger.LogDebug("Skipping token removed broadcast for map {MapId}", mapId);
            return;
        }

        var evt = new TokenRemovedEvent(mapId, tokenInstanceId, DateTime.UtcNow);
        var groupName = GetMapGroupName(mapId);
        await _hubContext.SendAsync(groupName, "TokenRemoved", evt, cancellationToken);
        _logger.LogInformation("Broadcast token removal for token {TokenId} on map {MapId}", tokenInstanceId, mapId);
    }

    public async Task SetMapPublicationStatusAsync(string mapId, PublicationStatusDto status, string userId, CancellationToken cancellationToken = default)
    {
        // Verify user has access to this map
        var existingMap = await _gameMapService.GetByIdAsync(mapId, userId, cancellationToken);
        if (existingMap == null)
        {
            throw new UnauthorizedAccessException("Map not found or access denied");
        }

        // Get the entity from repository to update
        var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
        if (map == null)
        {
            throw new InvalidOperationException("Map not found");
        }

        map.PublicationStatus = (PublicationStatusEntity)(int)status;
        map.UpdatedAt = DateTime.UtcNow;

        await _mapRepository.UpdateAsync(map, cancellationToken);

        // Broadcast the status change
        var evt = new MapStatusChangedEvent(mapId, status, DateTime.UtcNow);
        var groupName = GetMapGroupName(mapId);
        await _hubContext.SendAsync(groupName, "MapStatusChanged", evt, cancellationToken);

        _logger.LogInformation("Changed publication status for map {MapId} to {Status}", mapId, status);
    }

    public async Task<MapStateSnapshot?> GetMapStateSnapshotAsync(string mapId, string userId, CancellationToken cancellationToken = default)
    {
        // Verify user has access to this map
        var mapDto = await _gameMapService.GetByIdAsync(mapId, userId, cancellationToken);
        if (mapDto == null)
        {
            return null;
        }

        // Only return snapshot for Live maps
        if (mapDto.PublicationStatus != PublicationStatusDto.Live)
        {
            _logger.LogInformation("Snapshot requested for Draft map {MapId}, returning null", mapId);
            return null;
        }

        return new MapStateSnapshot(mapDto, DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the SignalR group name for a specific map.
    /// </summary>
    private static string GetMapGroupName(string mapId) => $"map_{mapId}";

    /// <summary>
    /// Determines if a broadcast should proceed based on throttle window.
    /// Implements sliding window throttling per map to reduce excessive broadcasts during rapid updates.
    /// </summary>
    /// <param name="mapId">The map ID to check throttle state for</param>
    /// <returns>True if broadcast should proceed, false if throttled</returns>
    private Task<bool> ShouldBroadcastAsync(string mapId)
    {
        var now = DateTime.UtcNow;

        // Ensure throttle state exists for this map
        if (!_throttleState.ContainsKey(mapId))
        {
            _throttleState.TryAdd(mapId, (now, new SemaphoreSlim(1, 1)));
            return Task.FromResult(true);
        }

        var (lastBroadcast, lockObj) = _throttleState[mapId];
        var timeSinceLastBroadcast = (now - lastBroadcast).TotalMilliseconds;

        if (timeSinceLastBroadcast >= _throttleWindowMs)
        {
            // Throttle window has elapsed, allow broadcast and update timestamp
            _throttleState[mapId] = (now, lockObj);
            return Task.FromResult(true);
        }

        // Still within throttle window, reject broadcast
        return Task.FromResult(false);
    }
}
