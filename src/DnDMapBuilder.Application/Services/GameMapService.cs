using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using PublicationStatusEntity = DnDMapBuilder.Data.Entities.PublicationStatus;
using PublicationStatusDto = DnDMapBuilder.Contracts.DTOs.PublicationStatus;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for GameMap business logic and CRUD operations.
/// </summary>
public class GameMapService : IGameMapService
{
    private readonly IGameMapRepository _mapRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IMapTokenInstanceRepository _tokenInstanceRepository;
    private readonly ILiveMapService? _liveMapService;

    public GameMapService(
        IGameMapRepository mapRepository,
        IMissionRepository missionRepository,
        ICampaignRepository campaignRepository,
        IMapTokenInstanceRepository tokenInstanceRepository,
        ILiveMapService? liveMapService = null)
    {
        _mapRepository = mapRepository;
        _missionRepository = missionRepository;
        _campaignRepository = campaignRepository;
        _tokenInstanceRepository = tokenInstanceRepository;
        _liveMapService = liveMapService;
    }

    /// <summary>
    /// Gets a GameMap by ID if user has access.
    /// </summary>
    /// <param name="id">The GameMap ID</param>
    /// <param name="userId">The requesting user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The GameMap DTO or null if not found or user has no access</returns>
    public async Task<GameMapDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetWithTokensAsync(id, cancellationToken);
        if (map == null)
        {
            return null;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId, cancellationToken))
        {
            return null;
        }

        return map.ToDto();
    }

    /// <summary>
    /// Gets all GameMaps for a specific mission.
    /// </summary>
    /// <param name="missionId">The mission ID</param>
    /// <param name="userId">The requesting user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of GameMap DTOs</returns>
    public async Task<IEnumerable<GameMapDto>> GetByMissionIdAsync(string missionId, string userId, CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToMapAsync(missionId, userId, cancellationToken))
        {
            return Enumerable.Empty<GameMapDto>();
        }

        var maps = await _mapRepository.GetByMissionIdAsync(missionId, cancellationToken);
        return maps.Select(m => m.ToDto());
    }

    /// <summary>
    /// Creates a new GameMap.
    /// </summary>
    /// <param name="request">Create map request</param>
    /// <param name="userId">The requesting user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created GameMap DTO</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user doesn't have permission</exception>
    public async Task<GameMapDto> CreateAsync(CreateMapRequest request, string userId, CancellationToken cancellationToken = default)
    {
        if (!await HasAccessToMapAsync(request.MissionId, userId, cancellationToken))
        {
            throw new UnauthorizedAccessException("You don't have permission to add maps to this mission.");
        }

        var map = new GameMap
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Rows = request.Rows,
            Cols = request.Cols,
            GridColor = request.GridColor,
            GridOpacity = request.GridOpacity,
            PublicationStatus = (PublicationStatusEntity)(int)request.PublicationStatus,
            MissionId = request.MissionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mapRepository.AddAsync(map, cancellationToken);
        return map.ToDto();
    }

    /// <summary>
    /// Updates an existing GameMap.
    /// </summary>
    /// <param name="id">The GameMap ID to update</param>
    /// <param name="request">Update map request</param>
    /// <param name="userId">The requesting user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated GameMap DTO or null if not found</returns>
    public async Task<GameMapDto?> UpdateAsync(string id, UpdateMapRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetByIdAsync(id, cancellationToken);
        if (map == null)
        {
            return null;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId, cancellationToken))
        {
            return null;
        }

        // Track if status changed to Live for broadcasting
        var statusChangedToLive = map.PublicationStatus != PublicationStatusEntity.Live &&
                                   request.PublicationStatus == PublicationStatusDto.Live;

        map.Name = request.Name;
        map.ImageUrl = request.ImageUrl;
        map.Rows = request.Rows;
        map.Cols = request.Cols;
        map.GridColor = request.GridColor;
        map.GridOpacity = request.GridOpacity;
        map.PublicationStatus = (PublicationStatusEntity)(int)request.PublicationStatus;
        map.UpdatedAt = DateTime.UtcNow;

        // Update tokens
        await _tokenInstanceRepository.DeleteByMapIdAsync(map.Id, cancellationToken);

        foreach (var tokenReq in request.Tokens)
        {
            var tokenInstance = new MapTokenInstance
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = tokenReq.TokenId,
                MapId = map.Id,
                X = tokenReq.X,
                Y = tokenReq.Y,
                CreatedAt = DateTime.UtcNow
            };
            await _tokenInstanceRepository.AddAsync(tokenInstance, cancellationToken);
        }

        await _mapRepository.UpdateAsync(map, cancellationToken);

        // Broadcast to live views
        if (_liveMapService != null)
        {
            // If status just changed to Live, broadcast the status change
            if (statusChangedToLive)
            {
                await _liveMapService.SetMapPublicationStatusAsync(id, PublicationStatusDto.Live, userId, cancellationToken);
            }
            else
            {
                // Otherwise broadcast the map update if already Live
                await _liveMapService.BroadcastMapUpdateAsync(id, cancellationToken);
            }
        }

        var updatedMap = await _mapRepository.GetWithTokensAsync(id, cancellationToken);
        return updatedMap?.ToDto();
    }

    /// <summary>
    /// Deletes a GameMap.
    /// </summary>
    /// <param name="id">The GameMap ID to delete</param>
    /// <param name="userId">The requesting user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion succeeded, false otherwise</returns>
    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var map = await _mapRepository.GetByIdAsync(id, cancellationToken);
        if (map == null)
        {
            return false;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId, cancellationToken))
        {
            return false;
        }

        await _mapRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    /// <summary>
    /// Checks if user has access to a map via mission and campaign ownership.
    /// </summary>
    private async Task<bool> HasAccessToMapAsync(string missionId, string userId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken);
        if (mission == null)
        {
            return false;
        }

        var campaign = await _campaignRepository.GetByIdAsync(mission.CampaignId, cancellationToken);
        return campaign != null && campaign.OwnerId == userId;
    }
}
