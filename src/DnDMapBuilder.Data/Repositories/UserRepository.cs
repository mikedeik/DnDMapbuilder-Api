using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Data.Repositories;

/// <summary>
/// Repository implementation for User entities.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(u => u.Status == "pending").ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(u => u.Status == "approved").ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(u => u.Role == role).ToListAsync(cancellationToken);
    }
}
