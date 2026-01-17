using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for authentication operations (user login and token generation).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token or null if login fails</returns>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return null; // User not found
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
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
}
