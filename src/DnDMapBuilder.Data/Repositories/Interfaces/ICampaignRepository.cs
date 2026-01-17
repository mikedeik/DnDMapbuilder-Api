using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository interface for Campaign entities with domain-specific queries.
/// </summary>
public interface ICampaignRepository : IGenericRepository<Campaign>
{
    /// <summary>
    /// Gets all campaigns owned by a specific user.
    /// </summary>
    /// <param name="ownerId">The owner's user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All campaigns owned by the user</returns>
    Task<IEnumerable<Campaign>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign with all its missions included.
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The campaign with missions or null if not found</returns>
    Task<Campaign?> GetWithMissionsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign with all related data (missions, maps, and tokens).
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The campaign with all related entities or null if not found</returns>
    Task<Campaign?> GetCompleteAsync(string id, CancellationToken cancellationToken = default);
}
