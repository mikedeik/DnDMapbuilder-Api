using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for game map business logic operations.
/// </summary>
public interface IGameMapService
{
    /// <summary>
    /// Gets a game map by ID with authorization check.
    /// </summary>
    /// <param name="id">The game map ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The game map DTO or null if not found or not authorized</returns>
    Task<GameMapDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all game maps for a specific mission with authorization check.
    /// </summary>
    /// <param name="missionId">The mission ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of game map DTOs in the mission</returns>
    Task<IEnumerable<GameMapDto>> GetByMissionIdAsync(string missionId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new game map in the specified mission.
    /// </summary>
    /// <param name="request">Map creation request</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created game map DTO</returns>
    Task<GameMapDto> CreateAsync(CreateMapRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing game map with authorization check.
    /// </summary>
    /// <param name="id">The game map ID to update</param>
    /// <param name="request">Map update request</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated game map DTO or null if not found or not authorized</returns>
    Task<GameMapDto?> UpdateAsync(string id, UpdateMapRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a game map with authorization check.
    /// </summary>
    /// <param name="id">The game map ID to delete</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found or not authorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
