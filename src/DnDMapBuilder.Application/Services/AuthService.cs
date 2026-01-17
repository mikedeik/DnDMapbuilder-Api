using BCrypt.Net;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return null; // User already exists
        }

        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUsername != null)
        {
            return null; // Username already taken
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "user",
            Status = "pending", // Requires admin approval
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponse(
            token,
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.Status
        );
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return null; // User not found
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null; // Invalid password
        }

        if (user.Status != "approved" && user.Role != "admin")
        {
            return null; // User not approved yet
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponse(
            token,
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.Status
        );
    }

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

    public async Task<IEnumerable<UserDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetPendingUsersAsync(cancellationToken);
        return users.Select(u => u.ToDto());
    }
}
