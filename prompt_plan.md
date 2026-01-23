# OAuth Authentication Implementation - Backend

## Overview

This plan outlines the implementation of Google and Apple OAuth authentication for the DnD Map Builder API. The implementation will allow users to sign in using their Google or Apple accounts as an alternative to email/password authentication.

---

## Architecture Decisions

### OAuth Flow
- **Flow Type**: Authorization Code Flow with PKCE (recommended for security)
- **Token Handling**: Backend validates OAuth tokens and issues its own JWT tokens
- **User Linking**: OAuth accounts can be linked to existing email-based accounts

### Database Changes
- Add OAuth provider fields to User entity
- Support multiple OAuth providers per user
- Store provider-specific user IDs for account linking

---

## Implementation Steps

---

### STEP-OAUTH-BE-001

**Status:** done
**Task:** Add Required NuGet Packages
**Files:** `src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj`

**Instructions:**

1. Add the following NuGet packages to the API project:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
```

2. For Apple Sign-In, we'll use a custom implementation since Apple requires JWT-based client secrets. Add:

```xml
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
```

3. Run `dotnet restore` to install packages

4. Verify build succeeds: `dotnet build`

---

### STEP-OAUTH-BE-002

**Status:** done
**Task:** Update User Entity for OAuth Support
**Files:** `src/DnDMapBuilder.Data/Entities/User.cs`

**Instructions:**

1. Read the current User.cs entity file

2. Add the following OAuth-related properties:

```csharp
/// <summary>
/// The OAuth provider used for authentication (null for email/password users)
/// Values: "google", "apple", null
/// </summary>
public string? OAuthProvider { get; set; }

/// <summary>
/// The unique identifier from the OAuth provider
/// </summary>
public string? OAuthProviderId { get; set; }

/// <summary>
/// URL to the user's profile picture from OAuth provider
/// </summary>
public string? ProfilePictureUrl { get; set; }

/// <summary>
/// Whether the email has been verified by the OAuth provider
/// </summary>
public bool EmailVerified { get; set; }
```

3. Ensure the entity still compiles correctly

4. Run `dotnet build` to verify

---

### STEP-OAUTH-BE-003

**Status:** done
**Task:** Create Database Migration for OAuth Fields
**Files:** `src/DnDMapBuilder.Data/Migrations/`

**Instructions:**

1. Navigate to the Data project directory

2. Create a new migration:
```bash
dotnet ef migrations add AddOAuthUserFields --project src/DnDMapBuilder.Data --startup-project src/DnDMapBuilder.Api
```

3. Review the generated migration file to ensure it:
   - Adds `OAuthProvider` column (nullable nvarchar)
   - Adds `OAuthProviderId` column (nullable nvarchar)
   - Adds `ProfilePictureUrl` column (nullable nvarchar)
   - Adds `EmailVerified` column (bit, default false)

4. Apply the migration:
```bash
dotnet ef database update --project src/DnDMapBuilder.Data --startup-project src/DnDMapBuilder.Api
```

5. Verify migration applied successfully

---

### STEP-OAUTH-BE-004

**Status:** done
**Task:** Create OAuth Configuration Settings
**Files:** `src/DnDMapBuilder.Api/appsettings.json`, `src/DnDMapBuilder.Api/appsettings.Development.json`

**Instructions:**

1. Add OAuth configuration section to appsettings.json:

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    },
    "Apple": {
      "ClientId": "",
      "TeamId": "",
      "KeyId": "",
      "PrivateKey": ""
    },
    "RedirectUri": "https://localhost:5001/api/v1/auth/oauth/callback",
    "FrontendRedirectUri": "https://localhost:3000/auth/callback"
  }
}
```

2. Create a configuration class in `src/DnDMapBuilder.Contracts/Configuration/OAuthSettings.cs`:

```csharp
namespace DnDMapBuilder.Contracts.Configuration;

public class OAuthSettings
{
    public GoogleSettings Google { get; set; } = new();
    public AppleSettings Apple { get; set; } = new();
    public string RedirectUri { get; set; } = string.Empty;
    public string FrontendRedirectUri { get; set; } = string.Empty;
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class AppleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
```

3. Register configuration in Program.cs:

```csharp
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuth"));
```

4. Verify build succeeds

---

### STEP-OAUTH-BE-005

**Status:** done
**Task:** Create OAuth DTOs and Contracts
**Files:** `src/DnDMapBuilder.Contracts/Requests/`, `src/DnDMapBuilder.Contracts/Responses/`

**Instructions:**

1. Create `OAuthLoginRequest.cs` in Requests folder:

```csharp
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
```

2. Create `OAuthTokenRequest.cs` for token-based flow (mobile/SPA):

```csharp
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
```

3. Create `OAuthUrlResponse.cs` in Responses folder:

```csharp
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
```

4. Verify build succeeds

---

### STEP-OAUTH-BE-006

**Status:** done
**Task:** Create IOAuthService Interface
**Files:** `src/DnDMapBuilder.Application/Interfaces/IOAuthService.cs`

**Instructions:**

1. Create a new interface file `IOAuthService.cs`:

```csharp
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
```

2. Verify build succeeds

---

### STEP-OAUTH-BE-007

**Status:** done
**Task:** Implement Google OAuth Token Validation
**Files:** `src/DnDMapBuilder.Application/Services/GoogleOAuthService.cs`

**Instructions:**

1. Create `GoogleOAuthService.cs`:

```csharp
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
```

2. Verify build succeeds

---

### STEP-OAUTH-BE-008

**Status:** done
**Task:** Implement Apple OAuth Token Validation
**Files:** `src/DnDMapBuilder.Application/Services/AppleOAuthService.cs`

**Instructions:**

1. Create `AppleOAuthService.cs`:

```csharp
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
```

2. Verify build succeeds

---

### STEP-OAUTH-BE-009

**Status:** done
**Task:** Implement Main OAuthService
**Files:** `src/DnDMapBuilder.Application/Services/OAuthService.cs`

**Instructions:**

1. Create `OAuthService.cs` that implements `IOAuthService`:

```csharp
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
        var token = _jwtService.GenerateToken(user);
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
```

2. Verify build succeeds

---

### STEP-OAUTH-BE-010

**Status:** done
**Task:** Update IUserRepository for OAuth Queries
**Files:** `src/DnDMapBuilder.Data/Repositories/Interfaces/IUserRepository.cs`, `src/DnDMapBuilder.Data/Repositories/UserRepository.cs`

**Instructions:**

1. Add new method to `IUserRepository.cs`:

```csharp
/// <summary>
/// Gets a user by OAuth provider and provider-specific ID.
/// </summary>
/// <param name="provider">OAuth provider name (google, apple)</param>
/// <param name="providerId">Provider-specific user ID</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The user or null if not found</returns>
Task<User?> GetByOAuthProviderAsync(string provider, string providerId, CancellationToken cancellationToken = default);
```

2. Implement in `UserRepository.cs`:

```csharp
public async Task<User?> GetByOAuthProviderAsync(string provider, string providerId, CancellationToken cancellationToken = default)
{
    return await _dbSet.AsNoTracking()
        .FirstOrDefaultAsync(u => u.OAuthProvider == provider && u.OAuthProviderId == providerId, cancellationToken);
}
```

3. Verify build succeeds

---

### STEP-OAUTH-BE-011

**Status:** done
**Task:** Add OAuth Endpoints to AuthController
**Files:** `src/DnDMapBuilder.Api/Controllers/AuthController.cs`

**Instructions:**

1. Read the current AuthController.cs file

2. Add IOAuthService dependency injection to constructor:

```csharp
private readonly IOAuthService _oAuthService;

public AuthController(IAuthService authService, IUserManagementService userManagementService, IOAuthService oAuthService)
{
    _authService = authService;
    _userManagementService = userManagementService;
    _oAuthService = oAuthService;
}
```

3. Add OAuth endpoints:

```csharp
/// <summary>
/// Get OAuth authorization URL for the specified provider
/// </summary>
[HttpGet("oauth/{provider}/url")]
public async Task<ActionResult<ApiResponse<OAuthUrlResponse>>> GetOAuthUrl(string provider, [FromQuery] string? redirectUri)
{
    try
    {
        var effectiveRedirectUri = redirectUri ?? $"{Request.Scheme}://{Request.Host}/api/v1/auth/oauth/callback";
        var response = await _oAuthService.GetAuthorizationUrlAsync(provider, effectiveRedirectUri);
        return Ok(new ApiResponse<OAuthUrlResponse>(true, response, $"{provider} authorization URL generated."));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new ApiResponse<OAuthUrlResponse>(false, null, ex.Message));
    }
}

/// <summary>
/// Handle OAuth callback with authorization code
/// </summary>
[HttpPost("oauth/callback")]
public async Task<ActionResult<ApiResponse<AuthResponse>>> OAuthCallback([FromBody] OAuthLoginRequest request, CancellationToken cancellationToken)
{
    try
    {
        var result = await _oAuthService.HandleOAuthCallbackAsync(request, cancellationToken);

        if (result == null)
        {
            return Unauthorized(new ApiResponse<AuthResponse>(false, null, "OAuth authentication failed."));
        }

        return Ok(new ApiResponse<AuthResponse>(true, result, "OAuth login successful."));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new ApiResponse<AuthResponse>(false, null, ex.Message));
    }
}

/// <summary>
/// Authenticate with OAuth ID token (for mobile/SPA clients)
/// </summary>
[HttpPost("oauth/token")]
public async Task<ActionResult<ApiResponse<AuthResponse>>> OAuthToken([FromBody] OAuthTokenRequest request, CancellationToken cancellationToken)
{
    try
    {
        var result = await _oAuthService.ValidateIdTokenAsync(request, cancellationToken);

        if (result == null)
        {
            return Unauthorized(new ApiResponse<AuthResponse>(false, null, "OAuth token validation failed."));
        }

        return Ok(new ApiResponse<AuthResponse>(true, result, "OAuth authentication successful."));
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new ApiResponse<AuthResponse>(false, null, ex.Message));
    }
}
```

4. Add required using statements at the top of the file

5. Verify build succeeds

---

### STEP-OAUTH-BE-012

**Status:** done
**Task:** Register OAuth Services in DI Container
**Files:** `src/DnDMapBuilder.Api/Program.cs`

**Instructions:**

1. Read the current Program.cs file

2. Add OAuth settings configuration:

```csharp
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuth"));
```

3. Register OAuth services:

```csharp
// OAuth Services
builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddHttpClient<AppleOAuthService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
```

4. Ensure the services are registered after other authentication services

5. Add required using statements

6. Verify build succeeds: `dotnet build`

---

### STEP-OAUTH-BE-013

**Status:** done
**Task:** Update UserDto to Include OAuth Fields
**Files:** `src/DnDMapBuilder.Contracts/DTOs/UserDto.cs`, `src/DnDMapBuilder.Application/Mappings/UserMappings.cs`

**Instructions:**

1. Update `UserDto.cs` to include OAuth fields:

```csharp
public record UserDto(
    string Id,
    string Username,
    string Email,
    string Role,
    string Status,
    string? OAuthProvider = null,
    string? ProfilePictureUrl = null,
    bool EmailVerified = false
);
```

2. Update `UserMappings.cs` to map the new fields:

```csharp
public static UserDto ToDto(this User user)
{
    return new UserDto(
        user.Id,
        user.Username,
        user.Email,
        user.Role,
        user.Status,
        user.OAuthProvider,
        user.ProfilePictureUrl,
        user.EmailVerified
    );
}
```

3. Verify build succeeds

---

### STEP-OAUTH-BE-014

**Status:** done
**Task:** Add CORS Configuration for OAuth
**Files:** `src/DnDMapBuilder.Api/Program.cs`

**Instructions:**

1. Ensure CORS policy allows the frontend origin:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["OAuth:FrontendRedirectUri"]?.TrimEnd('/') ?? "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

2. Apply CORS middleware (should already exist, verify it's before UseRouting):

```csharp
app.UseCors("AllowFrontend");
```

3. Verify build succeeds

---

### STEP-OAUTH-BE-015

**Status:** done
**Task:** Write Unit Tests for OAuth Services
**Files:** `tests/DnDMapBuilder.Application.Tests/Services/OAuthServiceTests.cs`

**Instructions:**

1. Create test file for OAuth service:

```csharp
using Xunit;
using Moq;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Tests.Services;

public class OAuthServiceTests
{
    [Fact]
    public async Task GetAuthorizationUrlAsync_Google_ReturnsValidUrl()
    {
        // Arrange & Act & Assert
        // Test implementation
    }

    [Fact]
    public async Task GetAuthorizationUrlAsync_Apple_ReturnsValidUrl()
    {
        // Test implementation
    }

    [Fact]
    public async Task GetAuthorizationUrlAsync_InvalidProvider_ThrowsArgumentException()
    {
        // Test implementation
    }

    [Fact]
    public async Task HandleOAuthCallbackAsync_ValidGoogleCode_ReturnsAuthResponse()
    {
        // Test implementation with mocked Google service
    }

    [Fact]
    public async Task ValidateIdTokenAsync_ValidGoogleToken_CreatesNewUser()
    {
        // Test implementation
    }

    [Fact]
    public async Task ValidateIdTokenAsync_ExistingUser_LinksOAuthProvider()
    {
        // Test implementation
    }
}
```

2. Run tests: `dotnet test`

---

### STEP-OAUTH-BE-016

**Status:** done
**Task:** Update API Documentation
**Files:** `API_DOCUMENTATION.md`

**Instructions:**

1. Add OAuth endpoints documentation:

```markdown
## OAuth Authentication

### Get OAuth Authorization URL
- **Endpoint**: `GET /api/v1/auth/oauth/{provider}/url`
- **Parameters**:
  - `provider` (path): OAuth provider ("google" or "apple")
  - `redirectUri` (query, optional): Custom redirect URI
- **Response**:
  ```json
  {
    "success": true,
    "data": {
      "authorizationUrl": "https://accounts.google.com/o/oauth2/v2/auth?...",
      "state": "random-state-string"
    }
  }
  ```

### OAuth Callback (Authorization Code Flow)
- **Endpoint**: `POST /api/v1/auth/oauth/callback`
- **Body**:
  ```json
  {
    "provider": "google",
    "code": "authorization-code-from-provider",
    "redirectUri": "https://your-app.com/callback"
  }
  ```
- **Response**: Same as login endpoint

### OAuth Token Validation (For Mobile/SPA)
- **Endpoint**: `POST /api/v1/auth/oauth/token`
- **Body**:
  ```json
  {
    "provider": "google",
    "idToken": "id-token-from-google-sdk"
  }
  ```
- **Response**: Same as login endpoint
```

2. Save changes

---

### STEP-OAUTH-BE-017

**Status:** done
**Task:** Final Build and Integration Testing
**Files:** All modified files

**Instructions:**

1. Run full build:
```bash
dotnet build
```

2. Run all tests:
```bash
dotnet test
```

3. Start the API:
```bash
dotnet run --project src/DnDMapBuilder.Api
```

4. Test OAuth URL generation:
```bash
curl http://localhost:5000/api/v1/auth/oauth/google/url
```

5. Verify Swagger documentation shows new endpoints

6. Fix any remaining issues

---

## Environment Variables Required

For production deployment, ensure these environment variables are set:

```
OAuth__Google__ClientId=your-google-client-id
OAuth__Google__ClientSecret=your-google-client-secret
OAuth__Apple__ClientId=your-apple-client-id
OAuth__Apple__TeamId=your-apple-team-id
OAuth__Apple__KeyId=your-apple-key-id
OAuth__Apple__PrivateKey=base64-encoded-private-key
OAuth__RedirectUri=https://api.yourdomain.com/api/v1/auth/oauth/callback
OAuth__FrontendRedirectUri=https://yourdomain.com
```

---

## Summary

**Total Steps:** 17

**Files Modified:**
- 1 .csproj file (packages)
- 1 Entity file
- 1 Migration
- 2 Configuration files
- 3 Contract files (DTOs)
- 1 Interface file
- 4 Service files
- 1 Controller file
- 1 DI registration file
- 1 Test file
- 1 Documentation file

**Key Features:**
- Google OAuth support
- Apple OAuth support
- Authorization code flow
- ID token validation (for mobile/SPA)
- Automatic user creation
- Account linking for existing users
- CSRF protection via state parameter
