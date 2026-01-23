using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for OAuth authentication operations
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Generates the OAuth authorization URL for the specified provider
    /// </summary>
    /// <param name="provider">OAuth provider (google or apple)</param>
    /// <param name="redirectUri">The redirect URI after authentication</param>
    /// <returns>Authorization URL and state parameter</returns>
    Task<OAuthUrlResponse> GetAuthorizationUrlAsync(string provider, string redirectUri);

    /// <summary>
    /// Handles OAuth callback and exchanges code for tokens
    /// </summary>
    /// <param name="request">OAuth login request with authorization code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Auth response with JWT token or null if failed</returns>
    Task<AuthResponse?> HandleOAuthCallbackAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an ID token and authenticates the user (for mobile/SPA clients)
    /// </summary>
    /// <param name="request">OAuth token request with ID token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Auth response with JWT token or null if failed</returns>
    Task<AuthResponse?> ValidateIdTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default);
}
