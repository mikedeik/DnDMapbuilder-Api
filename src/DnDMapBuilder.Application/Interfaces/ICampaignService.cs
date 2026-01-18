using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for campaign business logic operations.
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Gets a campaign by ID with authorization check.
    /// </summary>
    /// <param name="id">The campaign ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The campaign DTO or null if not found or not authorized</returns>
    Task<CampaignDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all campaigns owned by the specified user.
    /// </summary>
    /// <param name="userId">The owner's user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of campaign DTOs owned by the user</returns>
    Task<IEnumerable<CampaignDto>> GetUserCampaignsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new campaign for the specified user.
    /// </summary>
    /// <param name="request">Campaign creation request</param>
    /// <param name="userId">The user ID who will own the campaign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created campaign DTO</returns>
    Task<CampaignDto> CreateAsync(CreateCampaignRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing campaign with authorization check.
    /// </summary>
    /// <param name="id">The campaign ID to update</param>
    /// <param name="request">Campaign update request</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated campaign DTO or null if not found or not authorized</returns>
    Task<CampaignDto?> UpdateAsync(string id, UpdateCampaignRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a campaign with authorization check.
    /// </summary>
    /// <param name="id">The campaign ID to delete</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found or not authorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
