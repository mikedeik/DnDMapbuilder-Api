using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for GameMap entities with domain-specific queries.
/// </summary>
public interface IGameMapRepository : IGenericRepository<GameMap>
{
    /// <summary>
    /// Gets all game maps for a specific mission.
    /// </summary>
    /// <param name="missionId">The mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All maps in the mission</returns>
    Task<IEnumerable<GameMap>> GetByMissionIdAsync(string missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a game map with all its token instances included.
    /// </summary>
    /// <param name="id">The game map ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The game map with tokens or null if not found</returns>
    Task<GameMap?> GetWithTokensAsync(string id, CancellationToken cancellationToken = default);
}
