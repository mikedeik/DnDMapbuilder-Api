namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request for OAuth login using ID token (for mobile/SPA clients)
/// </summary>
public record OAuthTokenRequest(
    /// <summary>
    /// The OAuth provider ("google" or "apple")
    /// </summary>
    string Provider,

    /// <summary>
    /// The ID token received from OAuth provider
    /// </summary>
    string IdToken
);
