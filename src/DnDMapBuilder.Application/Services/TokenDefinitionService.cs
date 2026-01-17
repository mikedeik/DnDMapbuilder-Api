using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for TokenDefinition business logic and CRUD operations.
/// </summary>
public class TokenDefinitionService : ITokenDefinitionService
{
    private readonly ITokenDefinitionRepository _tokenRepository;

    public TokenDefinitionService(ITokenDefinitionRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    /// <summary>
    /// Gets a TokenDefinition by ID if it belongs to the user.
    /// </summary>
    /// <param name="id">The TokenDefinition ID</param>
    /// <param name="userId">The user ID to verify ownership</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The TokenDefinition DTO or null if not found or not owned by user</returns>
    public async Task<TokenDefinitionDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByIdAsync(id, cancellationToken);
        if (token == null || token.UserId != userId)
        {
            return null;
        }

        return token.ToDto();
    }

    /// <summary>
    /// Gets all TokenDefinitions created by a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user's TokenDefinition DTOs</returns>
    public async Task<IEnumerable<TokenDefinitionDto>> GetUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _tokenRepository.GetByUserIdAsync(userId, cancellationToken);
        return tokens.Select(t => t.ToDto());
    }

    /// <summary>
    /// Creates a new TokenDefinition.
    /// </summary>
    /// <param name="request">Create token request</param>
    /// <param name="userId">The user ID creating the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created TokenDefinition DTO</returns>
    public async Task<TokenDefinitionDto> CreateAsync(CreateTokenDefinitionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var token = new TokenDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Size = request.Size,
            Type = request.Type,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(token, cancellationToken);
        return token.ToDto();
    }

    /// <summary>
    /// Updates an existing TokenDefinition.
    /// </summary>
    /// <param name="id">The TokenDefinition ID to update</param>
    /// <param name="request">Update token request</param>
    /// <param name="userId">The user ID verifying ownership</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated TokenDefinition DTO or null if not found or not owned by user</returns>
    public async Task<TokenDefinitionDto?> UpdateAsync(string id, UpdateTokenDefinitionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByIdAsync(id, cancellationToken);
        if (token == null || token.UserId != userId)
        {
            return null;
        }

        token.Name = request.Name;
        token.ImageUrl = request.ImageUrl;
        token.Size = request.Size;
        token.Type = request.Type;
        token.UpdatedAt = DateTime.UtcNow;

        await _tokenRepository.UpdateAsync(token, cancellationToken);
        return token.ToDto();
    }

    /// <summary>
    /// Deletes a TokenDefinition.
    /// </summary>
    /// <param name="id">The TokenDefinition ID to delete</param>
    /// <param name="userId">The user ID verifying ownership</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion succeeded, false otherwise</returns>
    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByIdAsync(id, cancellationToken);
        if (token == null || token.UserId != userId)
        {
            return false;
        }

        await _tokenRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
