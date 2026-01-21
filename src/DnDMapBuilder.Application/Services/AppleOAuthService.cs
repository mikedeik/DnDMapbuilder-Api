using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using DnDMapBuilder.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for Apple OAuth token validation and user info retrieval
/// </summary>
public class AppleOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly OAuthSettings _settings;
    private readonly ILogger<AppleOAuthService> _logger;

    private const string AuthEndpoint = "https://appleid.apple.com/auth/authorize";
    private const string TokenEndpoint = "https://appleid.apple.com/auth/token";
    private const string KeysEndpoint = "https://appleid.apple.com/auth/keys";

    public AppleOAuthService(
        HttpClient httpClient,
        IOptions<OAuthSettings> settings,
        ILogger<AppleOAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generates Apple OAuth authorization URL
    /// </summary>
    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var scopes = Uri.EscapeDataString("name email");
        return $"{AuthEndpoint}?client_id={_settings.Apple.ClientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scopes}&state={state}&response_mode=form_post";
    }

    /// <summary>
    /// Exchanges authorization code for tokens
    /// </summary>
    public async Task<AppleTokenResponse?> ExchangeCodeForTokensAsync(string code, string redirectUri)
    {
        var clientSecret = GenerateClientSecret();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _settings.Apple.ClientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(TokenEndpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to exchange Apple auth code: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AppleTokenResponse>(json);
    }

    /// <summary>
    /// Validates an Apple ID token and returns user info
    /// </summary>
    public async Task<AppleUserInfo?> ValidateIdTokenAsync(string idToken)
    {
        try
        {
            // Get Apple's public keys
            var keysResponse = await _httpClient.GetStringAsync(KeysEndpoint);
            var keys = JsonSerializer.Deserialize<AppleKeysResponse>(keysResponse);

            if (keys?.Keys == null || keys.Keys.Count == 0)
            {
                _logger.LogError("Failed to get Apple public keys");
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);

            // Find the key that matches the token's kid
            var kid = jwtToken.Header.Kid;
            var key = keys.Keys.FirstOrDefault(k => k.Kid == kid);

            if (key == null)
            {
                _logger.LogError("No matching Apple key found for kid: {Kid}", kid);
                return null;
            }

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                Exponent = Base64UrlEncoder.DecodeBytes(key.E)
            });

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = _settings.Apple.ClientId,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };

            var principal = handler.ValidateToken(idToken, validationParameters, out _);

            return new AppleUserInfo
            {
                Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                EmailVerified = true // Apple verifies emails
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Apple ID token");
            return null;
        }
    }

    /// <summary>
    /// Generates Apple client secret (JWT signed with private key)
    /// </summary>
    private string GenerateClientSecret()
    {
        var now = DateTime.UtcNow;
        var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(Convert.FromBase64String(_settings.Apple.PrivateKey), out _);

        var signingCredentials = new SigningCredentials(
            new ECDsaSecurityKey(ecdsa) { KeyId = _settings.Apple.KeyId },
            SecurityAlgorithms.EcdsaSha256);

        var claims = new[]
        {
            new Claim("iss", _settings.Apple.TeamId),
            new Claim("iat", ((long)(now - DateTime.UnixEpoch).TotalSeconds).ToString()),
            new Claim("exp", ((long)(now.AddMonths(6) - DateTime.UnixEpoch).TotalSeconds).ToString()),
            new Claim("aud", "https://appleid.apple.com"),
            new Claim("sub", _settings.Apple.ClientId)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class AppleTokenResponse
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

public class AppleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? Name { get; set; }
}

public class AppleKeysResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("keys")]
    public List<AppleKey> Keys { get; set; } = new();
}

public class AppleKey
{
    [System.Text.Json.Serialization.JsonPropertyName("kid")]
    public string Kid { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("n")]
    public string N { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("e")]
    public string E { get; set; } = string.Empty;
}
