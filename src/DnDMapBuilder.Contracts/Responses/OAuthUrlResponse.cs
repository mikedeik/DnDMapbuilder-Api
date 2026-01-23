namespace DnDMapBuilder.Contracts.Responses;

/// <summary>
/// Response containing OAuth authorization URL
/// </summary>
public record OAuthUrlResponse(
    /// <summary>
    /// The URL to redirect the user to for OAuth authentication
    /// </summary>
    string AuthorizationUrl,

    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    string State
);
