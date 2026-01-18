using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for TokenDefinition entities.
/// </summary>
public class TokenDefinitionRepository : GenericRepository<TokenDefinition>, ITokenDefinitionRepository
{
    /// <summary>
    /// Initializes a new instance of the TokenDefinitionRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public TokenDefinitionRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<TokenDefinition>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TokenDefinition>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.Type == type)
            .ToListAsync(cancellationToken);
    }
}
