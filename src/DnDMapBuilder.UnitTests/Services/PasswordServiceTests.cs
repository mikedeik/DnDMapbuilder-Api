using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using FluentAssertions;
using Xunit;

namespace DnDMapBuilder.UnitTests.Services;

public class PasswordServiceTests
{
    private readonly IPasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonNullHash()
    {
        // Arrange
        const string password = "SecurePassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_WithSamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        const string password = "SecurePassword123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "SecurePassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "SecurePassword123!";
        const string wrongPassword = "WrongPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "SecurePassword123!";
        const string emptyPassword = "";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(emptyPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ThrowsException()
    {
        // Arrange
        const string password = "SecurePassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act & Assert
        var action = () => _passwordService.VerifyPassword(null, hash);
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ReturnsNonNullHash()
    {
        // Arrange
        const string password = "";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VerifyPassword_CanVerifyHashedEmptyPassword()
    {
        // Arrange
        const string password = "";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }
}
