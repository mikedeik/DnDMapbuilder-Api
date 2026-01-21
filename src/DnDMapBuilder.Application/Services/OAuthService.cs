using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.Configuration;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Main OAuth service that coordinates authentication across providers
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly GoogleOAuthService _googleService;
    private readonly AppleOAuthService _appleService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly OAuthSettings _settings;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        GoogleOAuthService googleService,
        AppleOAuthService appleService,
        IUserRepository userRepository,
        IJwtService jwtService,
        IOptions<OAuthSettings> settings,
        ILogger<OAuthService> logger)
    {
        _googleService = googleService;
        _appleService = appleService;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<OAuthUrlResponse> GetAuthorizationUrlAsync(string provider, string redirectUri)
    {
        var state = GenerateState();

        var authUrl = provider.ToLowerInvariant() switch
        {
            "google" => _googleService.GetAuthorizationUrl(redirectUri, state),
            "apple" => _appleService.GetAuthorizationUrl(redirectUri, state),
            _ => throw new ArgumentException($"Unsupported OAuth provider: {provider}")
        };

        return Task.FromResult(new OAuthUrlResponse(authUrl, state));
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> HandleOAuthCallbackAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default)
    {
        return request.Provider.ToLowerInvariant() switch
        {
            "google" => await HandleGoogleCallbackAsync(request.Code, request.RedirectUri, cancellationToken),
            "apple" => await HandleAppleCallbackAsync(request.Code, request.RedirectUri, cancellationToken),
            _ => throw new ArgumentException($"Unsupported OAuth provider: {request.Provider}")
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> ValidateIdTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default)
    {
        return request.Provider.ToLowerInvariant() switch
        {
            "google" => await ValidateGoogleIdTokenAsync(request.IdToken, cancellationToken),
            "apple" => await ValidateAppleIdTokenAsync(request.IdToken, cancellationToken),
            _ => throw new ArgumentException($"Unsupported OAuth provider: {request.Provider}")
        };
    }

    private async Task<AuthResponse?> HandleGoogleCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        var tokens = await _googleService.ExchangeCodeForTokensAsync(code, redirectUri);
        if (tokens == null) return null;

        var userInfo = await _googleService.ValidateIdTokenAsync(tokens.IdToken);
        if (userInfo == null) return null;

        return await CreateOrUpdateUserAndGenerateTokenAsync("google", userInfo.Id, userInfo.Email, userInfo.Name, userInfo.Picture, userInfo.EmailVerified, cancellationToken);
    }

    private async Task<AuthResponse?> HandleAppleCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        var tokens = await _appleService.ExchangeCodeForTokensAsync(code, redirectUri);
        if (tokens == null) return null;

        var userInfo = await _appleService.ValidateIdTokenAsync(tokens.IdToken);
        if (userInfo == null) return null;

        return await CreateOrUpdateUserAndGenerateTokenAsync("apple", userInfo.Id, userInfo.Email, userInfo.Name, null, userInfo.EmailVerified, cancellationToken);
    }

    private async Task<AuthResponse?> ValidateGoogleIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var userInfo = await _googleService.ValidateIdTokenAsync(idToken);
        if (userInfo == null) return null;

        return await CreateOrUpdateUserAndGenerateTokenAsync("google", userInfo.Id, userInfo.Email, userInfo.Name, userInfo.Picture, userInfo.EmailVerified, cancellationToken);
    }

    private async Task<AuthResponse?> ValidateAppleIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var userInfo = await _appleService.ValidateIdTokenAsync(idToken);
        if (userInfo == null) return null;

        return await CreateOrUpdateUserAndGenerateTokenAsync("apple", userInfo.Id, userInfo.Email, userInfo.Name, null, userInfo.EmailVerified, cancellationToken);
    }

    private async Task<AuthResponse?> CreateOrUpdateUserAndGenerateTokenAsync(
        string provider,
        string providerId,
        string email,
        string? name,
        string? picture,
        bool emailVerified,
        CancellationToken cancellationToken)
    {
        // Try to find existing user by OAuth provider ID
        var user = await _userRepository.GetByOAuthProviderAsync(provider, providerId, cancellationToken);

        // If not found, try to find by email
        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user != null)
            {
                // Link OAuth provider to existing account
                user.OAuthProvider = provider;
                user.OAuthProviderId = providerId;
                user.ProfilePictureUrl = picture;
                user.EmailVerified = emailVerified;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        // Create new user if not found
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = name ?? email.Split('@')[0],
                Email = email,
                PasswordHash = string.Empty, // OAuth users don't have passwords
                Role = "user",
                Status = "approved", // OAuth users are auto-approved
                OAuthProvider = provider,
                OAuthProviderId = providerId,
                ProfilePictureUrl = picture,
                EmailVerified = emailVerified,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, cancellationToken);
        }

        // Check if user is approved (may have been rejected previously)
        if (user.Status != "approved")
        {
            _logger.LogWarning("OAuth user {Email} attempted login but status is {Status}", email, user.Status);
            return null;
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
        var userDto = user.ToDto();

        return new AuthResponse(token, userDto.Id, userDto.Username, userDto.Email, userDto.Role, userDto.Status);
    }

    private static string GenerateState()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
