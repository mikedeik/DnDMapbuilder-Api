using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for MapTokenInstance entities.
/// </summary>
public class MapTokenInstanceRepository : GenericRepository<MapTokenInstance>, IMapTokenInstanceRepository
{
    /// <summary>
    /// Initializes a new instance of the MapTokenInstanceRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public MapTokenInstanceRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<MapTokenInstance>> GetByMapIdAsync(string mapId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.MapId == mapId)
            .Include(t => t.Token)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByMapIdAsync(string mapId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet.Where(t => t.MapId == mapId).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(tokens);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
