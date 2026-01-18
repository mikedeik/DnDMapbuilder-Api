using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for User entities with domain-specific queries.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user or null if not found</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with pending status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All pending users</returns>
    Task<IEnumerable<User>> GetPendingUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with a specific role.
    /// </summary>
    /// <param name="role">The role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All users with the specified role</returns>
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
}
