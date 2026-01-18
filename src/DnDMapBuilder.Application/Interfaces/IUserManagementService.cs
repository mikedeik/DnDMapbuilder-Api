using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for user management operations (registration, approval, profile management).
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Registers a new user with the provided credentials.
    /// </summary>
    /// <param name="request">Registration request with username, email, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user DTO or null if registration fails</returns>
    Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves or rejects a pending user registration.
    /// </summary>
    /// <param name="userId">The user ID to approve/reject</param>
    /// <param name="approved">True to approve, false to reject</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if approval succeeded, false otherwise</returns>
    Task<bool> ApproveUserAsync(string userId, bool approved, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with pending registration status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending users</returns>
    Task<IEnumerable<UserDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
}
