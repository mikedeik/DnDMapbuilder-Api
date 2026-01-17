using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DnDMapBuilder.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _authService = new AuthService(_mockUserRepository.Object, _mockJwtService.Object, _mockPasswordService.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "approved"
        };
        var expectedToken = "jwt_token_here";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);
        _mockJwtService.Setup(j => j.GenerateToken(user.Id, user.Email, user.Role))
            .Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@example.com", "password123");

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "wrongpassword");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "approved"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithPendingUserAndUserRole_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "pending"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithRejectedUserAndUserRole_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "rejected"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithPendingAdminUser_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@example.com", "password123");
        var user = new User
        {
            Id = "admin123",
            Email = "admin@example.com",
            Username = "admin",
            PasswordHash = "hashedpassword",
            Role = "admin",
            Status = "pending"
        };
        var expectedToken = "jwt_token_here";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);
        _mockJwtService.Setup(j => j.GenerateToken(user.Id, user.Email, user.Role))
            .Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.Role.Should().Be("admin");
    }

    [Fact]
    public async Task LoginAsync_UsesPasswordServiceForVerification()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "approved"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(false);

        // Act
        await _authService.LoginAsync(loginRequest);

        // Assert
        _mockPasswordService.Verify(
            p => p.VerifyPassword(loginRequest.Password, user.PasswordHash),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_GeneratesTokenWithCorrectParameters()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "user",
            Status = "approved"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email, default))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);
        _mockJwtService.Setup(j => j.GenerateToken(user.Id, user.Email, user.Role))
            .Returns("token");

        // Act
        await _authService.LoginAsync(loginRequest);

        // Assert
        _mockJwtService.Verify(
            j => j.GenerateToken(user.Id, user.Email, user.Role),
            Times.Once
        );
    }
}
