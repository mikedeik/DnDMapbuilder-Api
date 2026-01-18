using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for mission business logic operations.
/// </summary>
public interface IMissionService
{
    /// <summary>
    /// Gets a mission by ID with authorization check.
    /// </summary>
    /// <param name="id">The mission ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The mission DTO or null if not found or not authorized</returns>
    Task<MissionDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all missions for a specific campaign with authorization check.
    /// </summary>
    /// <param name="campaignId">The campaign ID</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of mission DTOs in the campaign</returns>
    Task<IEnumerable<MissionDto>> GetByCampaignIdAsync(string campaignId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new mission in the specified campaign.
    /// </summary>
    /// <param name="request">Mission creation request</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created mission DTO</returns>
    Task<MissionDto> CreateAsync(CreateMissionRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mission with authorization check.
    /// </summary>
    /// <param name="id">The mission ID to update</param>
    /// <param name="request">Mission update request</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated mission DTO or null if not found or not authorized</returns>
    Task<MissionDto?> UpdateAsync(string id, UpdateMissionRequest request, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mission with authorization check.
    /// </summary>
    /// <param name="id">The mission ID to delete</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found or not authorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
