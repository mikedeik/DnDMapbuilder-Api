using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Mission entities with domain-specific queries.
/// </summary>
public interface IMissionRepository : IGenericRepository<Mission>
{
    /// <summary>
    /// Gets all missions for a specific campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All missions in the campaign</returns>
    Task<IEnumerable<Mission>> GetByCampaignIdAsync(string campaignId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mission with all its game maps included.
    /// </summary>
    /// <param name="id">The mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The mission with maps or null if not found</returns>
    Task<Mission?> GetWithMapsAsync(string id, CancellationToken cancellationToken = default);
}
