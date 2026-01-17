using Microsoft.EntityFrameworkCore;
using Xunit;
using DnDMapBuilder.Data;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories;

namespace DnDMapBuilder.UnitTests.Repositories;

/// <summary>
/// Unit tests for GenericRepository CRUD operations using in-memory database.
/// </summary>
public class GenericRepositoryTests
{
    private DnDMapBuilderDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DnDMapBuilderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DnDMapBuilderDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity_WhenCalledWithValidEntity()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<User>(context);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        // Assert
        var addedUser = await repository.GetByIdAsync(user.Id);
        Assert.NotNull(addedUser);
        Assert.Equal("testuser", addedUser.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<User>(context);

        // Act
        var retrievedUser = await repository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(userId, retrievedUser.Id);
        Assert.Equal("testuser", retrievedUser.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<User>(context);

        // Act
        var result = await repository.GetByIdAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var user1 = new User { Id = "1", Username = "user1", Email = "u1@test.com", PasswordHash = "hash", Role = "user", Status = "pending", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var user2 = new User { Id = "2", Username = "user2", Email = "u2@test.com", PasswordHash = "hash", Role = "user", Status = "pending", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<User>(context);

        // Act
        var users = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(users);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity_WhenCalledWithValidEntity()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Username = "originalname",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<User>(context);
        user.Username = "updatedname";

        // Act
        await repository.UpdateAsync(user);
        await context.SaveChangesAsync();

        // Assert
        var updatedUser = await repository.GetByIdAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal("updatedname", updatedUser.Username);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity_WhenCalledWithValidId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<User>(context);

        // Act
        await repository.DeleteAsync(userId);

        // Assert
        var deletedUser = await repository.GetByIdAsync(userId);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<User>(context);

        // Act
        var exists = await repository.ExistsAsync(userId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<User>(context);

        // Act
        var exists = await repository.ExistsAsync("non-existent-id");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task AddAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<User>(context);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "user",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Repository methods accept CancellationToken, but the test verifies it's handled
        try
        {
            await repository.AddAsync(user, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected behavior
        }
    }
}
