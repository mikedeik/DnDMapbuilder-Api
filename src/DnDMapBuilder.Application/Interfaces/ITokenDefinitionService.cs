using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for token definition business logic operations.
/// </summary>
public interface ITokenDefinitionService
{
    /// <summary>
    /// Gets a token definition by ID with authorization check.
    /// </summary>
    /// <param name="id">The token definition ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token definition DTO or null if not found or not authorized</returns>
    Task<TokenDefinitionDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all token definitions for a specific user.
    /// </summary>
    /// <param name="userId">The user ID owner of the tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of token definition DTOs owned by the user</returns>
    Task<IEnumerable<TokenDefinitionDto>> GetUserTokensAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new token definition for the specified user.
    /// </summary>
    /// <param name="request">Token definition creation request</param>
    /// <param name="userId">The user ID who will own the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created token definition DTO</returns>
    Task<TokenDefinitionDto> CreateAsync(CreateTokenDefinitionRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing token definition with authorization check.
    /// </summary>
    /// <param name="id">The token definition ID to update</param>
    /// <param name="request">Token definition update request</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated token definition DTO or null if not found or not authorized</returns>
    Task<TokenDefinitionDto?> UpdateAsync(string id, UpdateTokenDefinitionRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a token definition with authorization check.
    /// </summary>
    /// <param name="id">The token definition ID to delete</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found or not authorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
