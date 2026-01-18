using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for user management operations including registration, approval, and user querying.
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public UserManagementService(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Registers a new user with the provided credentials.
    /// </summary>
    /// <param name="request">Registration request with username, email, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user DTO or null if registration fails</returns>
    public async Task<UserDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists by email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return null; // User already exists
        }

        // Check if username is already taken
        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUsername != null)
        {
            return null; // Username already taken
        }

        // Create new user with hashed password
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = "user",
            Status = "pending", // Requires admin approval
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return user.ToDto();
    }

    /// <summary>
    /// Approves or rejects a pending user registration.
    /// </summary>
    /// <param name="userId">The user ID to approve/reject</param>
    /// <param name="approved">True to approve, false to reject</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if approval succeeded, false otherwise</returns>
    public async Task<bool> ApproveUserAsync(string userId, bool approved, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.Status = approved ? "approved" : "rejected";
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets all users with pending registration status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending users</returns>
    public async Task<IEnumerable<UserDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetPendingUsersAsync(cancellationToken);
        return users.Select(u => u.ToDto());
    }
}
