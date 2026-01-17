using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations including user registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the provided credentials.
    /// </summary>
    /// <param name="request">Registration request with username, email, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token or null if registration fails</returns>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login request with username and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token or null if login fails</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

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
