using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for TokenDefinition entities with domain-specific queries.
/// </summary>
public interface ITokenDefinitionRepository : IGenericRepository<TokenDefinition>
{
    /// <summary>
    /// Gets all token definitions for a specific user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All token definitions owned by the user</returns>
    Task<IEnumerable<TokenDefinition>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all token definitions of a specific type.
    /// </summary>
    /// <param name="type">The token type (e.g., "player" or "enemy")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All tokens of the specified type</returns>
    Task<IEnumerable<TokenDefinition>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
}
