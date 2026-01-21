namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request for OAuth-based login/registration
/// </summary>
public record OAuthLoginRequest(
    /// <summary>
    /// The OAuth provider ("google" or "apple")
    /// </summary>
    string Provider,

    /// <summary>
    /// The authorization code received from OAuth provider
    /// </summary>
    string Code,

    /// <summary>
    /// The redirect URI used in the OAuth flow (for validation)
    /// </summary>
    string RedirectUri
);
