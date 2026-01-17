using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for Campaign entities.
/// </summary>
public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    /// <summary>
    /// Initializes a new instance of the CampaignRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public CampaignRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<Campaign>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.OwnerId == ownerId)
            .Include(c => c.Missions)
            .ToListAsync(cancellationToken);
    }

    public async Task<Campaign?> GetWithMissionsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Missions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Campaign?> GetCompleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Missions)
                .ThenInclude(m => m.Maps)
                    .ThenInclude(map => map.Tokens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
