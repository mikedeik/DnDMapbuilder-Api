using System.Text.Json;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for Google OAuth token validation and user info retrieval
/// </summary>
public class GoogleOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly OAuthSettings _settings;
    private readonly ILogger<GoogleOAuthService> _logger;

    private const string TokenInfoEndpoint = "https://oauth2.googleapis.com/tokeninfo";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";

    public GoogleOAuthService(
        HttpClient httpClient,
        IOptions<OAuthSettings> settings,
        ILogger<GoogleOAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generates Google OAuth authorization URL
    /// </summary>
    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var scopes = Uri.EscapeDataString("openid email profile");
        return $"{AuthEndpoint}?client_id={_settings.Google.ClientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scopes}&state={state}&access_type=offline&prompt=consent";
    }

    /// <summary>
    /// Exchanges authorization code for tokens
    /// </summary>
    public async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code, string redirectUri)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _settings.Google.ClientId,
            ["client_secret"] = _settings.Google.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to exchange Google auth code: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleTokenResponse>(json);
    }

    /// <summary>
    /// Validates an ID token and returns user info
    /// </summary>
    public async Task<GoogleUserInfo?> ValidateIdTokenAsync(string idToken)
    {
        var response = await _httpClient.GetAsync($"{TokenInfoEndpoint}?id_token={idToken}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to validate Google ID token: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json);

        // Verify the token is for our app
        if (tokenInfo?.Aud != _settings.Google.ClientId)
        {
            _logger.LogError("Google ID token audience mismatch");
            return null;
        }

        return new GoogleUserInfo
        {
            Id = tokenInfo.Sub,
            Email = tokenInfo.Email,
            EmailVerified = tokenInfo.EmailVerified,
            Name = tokenInfo.Name,
            Picture = tokenInfo.Picture
        };
    }

    /// <summary>
    /// Gets user info using access token
    /// </summary>
    public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(UserInfoEndpoint);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get Google user info: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleUserInfo>(json);
    }
}

public class GoogleTokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class GoogleTokenInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("aud")]
    public string Aud { get; set; } = string.Empty;
}

public class GoogleUserInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("sub")]
    public string Id { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("picture")]
    public string? Picture { get; set; }
}
