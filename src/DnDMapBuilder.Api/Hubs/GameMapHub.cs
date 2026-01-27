using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DnDMapBuilder.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time game map updates and live view broadcasting.
/// Handles WebSocket connections from DMs and live view clients.
/// </summary>
[Authorize]
public class GameMapHub : Hub
{
    private readonly ILogger<GameMapHub> _logger;
    private readonly IGameMapService _gameMapService;
    private readonly ITelemetryService _telemetry;

    public GameMapHub(ILogger<GameMapHub> logger, IGameMapService gameMapService, ITelemetryService telemetry)
    {
        _logger = logger;
        _gameMapService = gameMapService;
        _telemetry = telemetry;
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var connectionId = Context.ConnectionId;
        _telemetry.RecordSignalRConnection(connected: true);
        _logger.LogInformation("User {UserId} connected to GameMapHub (ConnectionId: {ConnectionId})", userId, connectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var connectionId = Context.ConnectionId;
        _telemetry.RecordSignalRConnection(connected: false);
        _logger.LogInformation("User {UserId} disconnected from GameMapHub (ConnectionId: {ConnectionId})", userId, connectionId);
        if (exception != null)
        {
            _logger.LogError(exception, "Error in GameMapHub connection for user {UserId}", userId);
        }
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows a client to subscribe to updates for a specific game map.
    /// Client must have access to the map's campaign to join the group.
    /// </summary>
    /// <param name="mapId">The ID of the map to subscribe to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="HubException">Thrown if user is unauthorized or map not found</exception>
    public async Task JoinMapGroup(string mapId, CancellationToken cancellationToken = default)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized join attempt to map group {MapId} - no userId in claims", mapId);
            throw new HubException("Unauthorized");
        }

        // Verify user has access to this map
        var map = await _gameMapService.GetByIdAsync(mapId, userId, cancellationToken);
        if (map == null)
        {
            _logger.LogWarning("User {UserId} denied access to map {MapId} - map not found or access denied", userId, mapId);
            throw new HubException("Map not found or access denied");
        }

        var groupName = GetMapGroupName(mapId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName, cancellationToken);
        _logger.LogInformation("User {UserId} joined map group {MapId} (ConnectionId: {ConnectionId})", userId, mapId, Context.ConnectionId);
    }

    /// <summary>
    /// Allows a client to unsubscribe from updates for a specific game map.
    /// </summary>
    /// <param name="mapId">The ID of the map to unsubscribe from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task LeaveMapGroup(string mapId, CancellationToken cancellationToken = default)
    {
        var groupName = GetMapGroupName(mapId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName, cancellationToken);
        _logger.LogInformation("Connection {ConnectionId} left map group {MapId}", Context.ConnectionId, mapId);
    }

    /// <summary>
    /// Gets the SignalR group name for a specific map.
    /// </summary>
    private static string GetMapGroupName(string mapId) => $"map_{mapId}";
}
