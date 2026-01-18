using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for Mission entities.
/// </summary>
public class MissionRepository : GenericRepository<Mission>, IMissionRepository
{
    /// <summary>
    /// Initializes a new instance of the MissionRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public MissionRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<Mission>> GetByCampaignIdAsync(string campaignId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.CampaignId == campaignId)
            .Include(m => m.Maps)
            .ToListAsync(cancellationToken);
    }

    public async Task<Mission?> GetWithMapsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Maps)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}
