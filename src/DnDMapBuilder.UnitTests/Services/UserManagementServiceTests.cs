using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DnDMapBuilder.UnitTests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly UserManagementService _userManagementService;

    public UserManagementServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _userManagementService = new UserManagementService(_mockUserRepository.Object, _mockPasswordService.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidCredentials_CreatesUserSuccessfully()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "new@example.com", "password123");
        const string hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(registerRequest.Username, default))
            .ReturnsAsync((User)null);
        _mockPasswordService.Setup(p => p.HashPassword(registerRequest.Password))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>((user, ct) => Task.FromResult(user));

        // Act
        var result = await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(registerRequest.Username);
        result.Email.Should().Be(registerRequest.Email);
        result.Role.Should().Be("user");
        result.Status.Should().Be("pending");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsNull()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "existing@example.com", "password123");
        var existingUser = new User
        {
            Email = registerRequest.Email,
            Username = "existinguser"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsNull()
    {
        // Arrange
        var registerRequest = new RegisterRequest("existinguser", "new@example.com", "password123");
        var existingUser = new User
        {
            Username = registerRequest.Username,
            Email = "existing@example.com"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(registerRequest.Username, default))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_UsesPasswordServiceToHashPassword()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "new@example.com", "password123");
        const string hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(registerRequest.Username, default))
            .ReturnsAsync((User)null);
        _mockPasswordService.Setup(p => p.HashPassword(registerRequest.Password))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>((user, ct) => Task.FromResult(user));

        // Act
        await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        _mockPasswordService.Verify(
            p => p.HashPassword(registerRequest.Password),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_CallsRepositoryAddAsync()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "new@example.com", "password123");
        const string hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(registerRequest.Username, default))
            .ReturnsAsync((User)null);
        _mockPasswordService.Setup(p => p.HashPassword(registerRequest.Password))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>((user, ct) => Task.FromResult(user));

        // Act
        await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        _mockUserRepository.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ApproveUserAsync_WithValidUserId_ApprovesUser()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Status = "pending"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.ApproveUserAsync(userId, true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveUserAsync_WithValidUserId_RejectsUser()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Status = "pending"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.ApproveUserAsync(userId, false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveUserAsync_WithNonExistentUserId_ReturnsFalse()
    {
        // Arrange
        var userId = "nonexistent";

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userManagementService.ApproveUserAsync(userId, true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveUserAsync_UpdatesUserStatus()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Status = "pending"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userManagementService.ApproveUserAsync(userId, true);

        // Assert
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Id == userId && u.Status == "approved"), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ApproveUserAsync_SetStatusToRejectedWhenApprovedIsFalse()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Status = "pending"
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userManagementService.ApproveUserAsync(userId, false);

        // Assert
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Id == userId && u.Status == "rejected"), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetPendingUsersAsync_ReturnsPendingUsers()
    {
        // Arrange
        var pendingUsers = new List<User>
        {
            new User { Id = "user1", Username = "user1", Email = "user1@example.com", Status = "pending", Role = "user" },
            new User { Id = "user2", Username = "user2", Email = "user2@example.com", Status = "pending", Role = "user" }
        };

        _mockUserRepository.Setup(r => r.GetPendingUsersAsync(default))
            .ReturnsAsync(pendingUsers);

        // Act
        var result = await _userManagementService.GetPendingUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.Status.Should().Be("pending"));
    }

    [Fact]
    public async Task GetPendingUsersAsync_ReturnsEmptyListWhenNoPendingUsers()
    {
        // Arrange
        var emptyList = new List<User>();

        _mockUserRepository.Setup(r => r.GetPendingUsersAsync(default))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _userManagementService.GetPendingUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_SetsInitialStatusToPending()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "new@example.com", "password123");
        const string hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerRequest.Email, default))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(registerRequest.Username, default))
            .ReturnsAsync((User)null);
        _mockPasswordService.Setup(p => p.HashPassword(registerRequest.Password))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns<User, CancellationToken>((user, ct) => Task.FromResult(user));

        // Act
        var result = await _userManagementService.RegisterAsync(registerRequest);

        // Assert
        result.Status.Should().Be("pending");
    }
}
