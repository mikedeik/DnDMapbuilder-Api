using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for MapTokenInstance entities with domain-specific queries.
/// </summary>
public interface IMapTokenInstanceRepository : IGenericRepository<MapTokenInstance>
{
    /// <summary>
    /// Gets all token instances on a specific game map.
    /// </summary>
    /// <param name="mapId">The game map ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All token instances on the map</returns>
    Task<IEnumerable<MapTokenInstance>> GetByMapIdAsync(string mapId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all token instances on a specific game map.
    /// </summary>
    /// <param name="mapId">The game map ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteByMapIdAsync(string mapId, CancellationToken cancellationToken = default);
}
