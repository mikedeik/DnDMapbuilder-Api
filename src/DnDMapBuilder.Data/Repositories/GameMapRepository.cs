using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for GameMap entities.
/// </summary>
public class GameMapRepository : GenericRepository<GameMap>, IGameMapRepository
{
    /// <summary>
    /// Initializes a new instance of the GameMapRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public GameMapRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<GameMap>> GetByMissionIdAsync(string missionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.MissionId == missionId)
            .Include(m => m.Tokens)
            .ToListAsync(cancellationToken);
    }

    public async Task<GameMap?> GetWithTokensAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Tokens)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}
