using Asp.Versioning;
using DnDMapBuilder.Api.Controllers;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace DnDMapBuilder.UnitTests.Controllers;

public class AuthControllerOAuthTests
{
    private readonly IAuthService _authService;
    private readonly IUserManagementService _userManagementService;
    private readonly IOAuthService _oAuthService;
    private readonly AuthController _sut;

    public AuthControllerOAuthTests()
    {
        _authService = Substitute.For<IAuthService>();
        _userManagementService = Substitute.For<IUserManagementService>();
        _oAuthService = Substitute.For<IOAuthService>();

        _sut = new AuthController(_authService, _userManagementService, _oAuthService);
    }

    #region GetOAuthUrl Tests

    [Fact]
    public async Task GetOAuthUrl_WithValidGoogleProvider_ReturnsOkWithAuthorizationUrl()
    {
        // Arrange
        const string provider = "google";
        const string redirectUri = "https://localhost:3000/callback";
        var expectedResponse = new OAuthUrlResponse(
            "https://accounts.google.com/o/oauth2/v2/auth?...",
            "test-state-123"
        );

        _oAuthService.GetAuthorizationUrlAsync(provider, redirectUri)
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetOAuthUrl(provider, redirectUri);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<OAuthUrlResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.AuthorizationUrl.Should().Contain("accounts.google.com");
    }

    [Fact]
    public async Task GetOAuthUrl_WithInvalidProvider_ReturnsBadRequest()
    {
        // Arrange
        const string provider = "github";
        const string redirectUri = "https://localhost:3000/callback";
        var exceptionMessage = "Unsupported OAuth provider: github";

        _oAuthService.GetAuthorizationUrlAsync(provider, redirectUri)
            .Returns(Task.FromException<OAuthUrlResponse>(new ArgumentException(exceptionMessage)));

        // Act
        var result = await _sut.GetOAuthUrl(provider, redirectUri);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<OAuthUrlResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("Unsupported OAuth provider");
    }

    [Fact]
    public async Task GetOAuthUrl_WithOAuthServiceException_ReturnsBadRequest()
    {
        // Arrange
        const string provider = "invalid";
        const string redirectUri = "https://localhost:3000/callback";

        _oAuthService.GetAuthorizationUrlAsync(provider, redirectUri)
            .Returns(Task.FromException<OAuthUrlResponse>(new ArgumentException("Invalid provider")));

        // Act
        var result = await _sut.GetOAuthUrl(provider, redirectUri);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    #endregion

    #region OAuthCallback Tests

    [Fact]
    public async Task OAuthCallback_WithValidGoogleCode_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new OAuthLoginRequest("google", "auth_code_123", "https://localhost:3000/callback");
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_123",
            "user_id_123",
            "testuser",
            "user@example.com",
            "user",
            "approved"
        );

        _oAuthService.HandleOAuthCallbackAsync(request, Arg.Any<CancellationToken>())
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthCallback(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<AuthResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Token.Should().Be("jwt_token_123");
        apiResponse.Data.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task OAuthCallback_WithValidAppleCode_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new OAuthLoginRequest("apple", "auth_code_456", "https://localhost:3000/callback");
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_456",
            "user_id_456",
            "appleuser",
            "apple@example.com",
            "user",
            "approved"
        );

        _oAuthService.HandleOAuthCallbackAsync(request, Arg.Any<CancellationToken>())
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthCallback(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<AuthResponse>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Email.Should().Be("apple@example.com");
    }

    [Fact]
    public async Task OAuthCallback_WithInvalidCode_ReturnsUnauthorized()
    {
        // Arrange
        var request = new OAuthLoginRequest("google", "invalid_code", "https://localhost:3000/callback");

        _oAuthService.HandleOAuthCallbackAsync(request, Arg.Any<CancellationToken>())
            .Returns((AuthResponse?)null);

        // Act
        var result = await _sut.OAuthCallback(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);

        var apiResponse = unauthorizedResult.Value as ApiResponse<AuthResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("OAuth authentication failed");
    }

    [Fact]
    public async Task OAuthCallback_WithUnsupportedProvider_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest("facebook", "code", "https://localhost:3000/callback");

        _oAuthService.HandleOAuthCallbackAsync(request, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse?>(new ArgumentException("Unsupported OAuth provider: facebook")));

        // Act
        var result = await _sut.OAuthCallback(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<AuthResponse>;
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task OAuthCallback_WithOAuthServiceException_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest("google", "code", "https://localhost:3000/callback");

        _oAuthService.HandleOAuthCallbackAsync(request, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse?>(new ArgumentException("OAuth service error")));

        // Act
        var result = await _sut.OAuthCallback(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    #endregion

    #region OAuthToken Tests

    [Fact]
    public async Task OAuthToken_WithValidGoogleIdToken_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new OAuthTokenRequest("google", "id_token_123");
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_123",
            "user_id_123",
            "testuser",
            "user@example.com",
            "user",
            "approved"
        );

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<AuthResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Token.Should().Be("jwt_token_123");
        apiResponse.Message.Should().Contain("OAuth authentication successful");
    }

    [Fact]
    public async Task OAuthToken_WithValidAppleIdToken_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new OAuthTokenRequest("apple", "id_token_456");
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_456",
            "user_id_456",
            "appleuser",
            "apple@example.com",
            "user",
            "approved"
        );

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<AuthResponse>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.Email.Should().Be("apple@example.com");
    }

    [Fact]
    public async Task OAuthToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new OAuthTokenRequest("google", "invalid_token");

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns((AuthResponse?)null);

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);

        var apiResponse = unauthorizedResult.Value as ApiResponse<AuthResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("OAuth token validation failed");
    }

    [Fact]
    public async Task OAuthToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new OAuthTokenRequest("google", "expired_token");

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns((AuthResponse?)null);

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task OAuthToken_WithUnsupportedProvider_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthTokenRequest("linkedin", "token");

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse?>(new ArgumentException("Unsupported OAuth provider: linkedin")));

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task OAuthToken_WithOAuthServiceException_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthTokenRequest("google", "token");

        _oAuthService.ValidateIdTokenAsync(request, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse?>(new ArgumentException("Token validation error")));

        // Act
        var result = await _sut.OAuthToken(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task OAuthCallback_CancellationTokenIsPassedCorrectly()
    {
        // Arrange
        var request = new OAuthLoginRequest("google", "code", "https://localhost:3000/callback");
        var cancellationToken = new CancellationToken();
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_123",
            "user_id_123",
            "testuser",
            "user@example.com",
            "user",
            "approved"
        );

        _oAuthService.HandleOAuthCallbackAsync(request, cancellationToken)
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthCallback(request, cancellationToken);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        // Verify cancellation token was passed
        await _oAuthService.Received(1).HandleOAuthCallbackAsync(request, cancellationToken);
    }

    [Fact]
    public async Task OAuthToken_CancellationTokenIsPassedCorrectly()
    {
        // Arrange
        var request = new OAuthTokenRequest("google", "token");
        var cancellationToken = new CancellationToken();
        var expectedAuthResponse = new AuthResponse(
            "jwt_token_123",
            "user_id_123",
            "testuser",
            "user@example.com",
            "user",
            "approved"
        );

        _oAuthService.ValidateIdTokenAsync(request, cancellationToken)
            .Returns(expectedAuthResponse);

        // Act
        var result = await _sut.OAuthToken(request, cancellationToken);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        // Verify cancellation token was passed
        await _oAuthService.Received(1).ValidateIdTokenAsync(request, cancellationToken);
    }

    [Fact]
    public async Task GetOAuthUrl_ProviderNormalizationWorks()
    {
        // Arrange
        const string provider = "GOOGLE";
        const string redirectUri = "https://localhost:3000/callback";
        var expectedResponse = new OAuthUrlResponse(
            "https://accounts.google.com/o/oauth2/v2/auth?...",
            "test-state-123"
        );

        _oAuthService.GetAuthorizationUrlAsync(Arg.Any<string>(), redirectUri)
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetOAuthUrl(provider, redirectUri);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        // Verify provider normalization
        await _oAuthService.Received(1).GetAuthorizationUrlAsync(Arg.Any<string>(), redirectUri);
    }

    #endregion
}
