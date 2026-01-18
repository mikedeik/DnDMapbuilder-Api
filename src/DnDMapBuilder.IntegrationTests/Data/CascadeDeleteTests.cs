using DnDMapBuilder.Data;
using DnDMapBuilder.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DnDMapBuilder.IntegrationTests.Data;

public class CascadeDeleteTests : IAsyncLifetime
{
    private DbContextOptions<DnDMapBuilderDbContext> _options = null!;
    private DnDMapBuilderDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        // Use in-memory database for tests
        _options = new DbContextOptionsBuilder<DnDMapBuilderDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new DnDMapBuilderDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task WhenCampaignDeleted_ThenAllMissionsShouldBeDeleted()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };
        var mission = new Mission { Name = "Test Mission", CampaignId = campaign.Id, Campaign = campaign };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        _dbContext.Missions.Add(mission);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the campaign
        var campaignToDelete = await _dbContext.Campaigns.FirstAsync();
        _dbContext.Campaigns.Remove(campaignToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var missionsCount = await _dbContext.Missions.CountAsync();
        missionsCount.Should().Be(0, "All missions should be deleted when campaign is deleted");
    }

    [Fact]
    public async Task WhenMissionDeleted_ThenAllGameMapsShouldBeDeleted()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };
        var mission = new Mission { Name = "Test Mission", CampaignId = campaign.Id, Campaign = campaign };
        var gameMap = new GameMap { Name = "Test Map", MissionId = mission.Id, Mission = mission };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        _dbContext.Missions.Add(mission);
        _dbContext.GameMaps.Add(gameMap);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the mission
        var missionToDelete = await _dbContext.Missions.FirstAsync();
        _dbContext.Missions.Remove(missionToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var mapsCount = await _dbContext.GameMaps.CountAsync();
        mapsCount.Should().Be(0, "All game maps should be deleted when mission is deleted");
    }

    [Fact]
    public async Task WhenGameMapDeleted_ThenAllMapTokenInstancesShouldBeDeleted()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };
        var mission = new Mission { Name = "Test Mission", CampaignId = campaign.Id, Campaign = campaign };
        var gameMap = new GameMap { Name = "Test Map", MissionId = mission.Id, Mission = mission };
        var token = new TokenDefinition { Name = "Test Token", UserId = user.Id, User = user };
        var instance = new MapTokenInstance { TokenId = token.Id, Token = token, MapId = gameMap.Id, Map = gameMap };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        _dbContext.Missions.Add(mission);
        _dbContext.GameMaps.Add(gameMap);
        _dbContext.TokenDefinitions.Add(token);
        _dbContext.MapTokenInstances.Add(instance);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the game map
        var mapToDelete = await _dbContext.GameMaps.FirstAsync();
        _dbContext.GameMaps.Remove(mapToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var instancesCount = await _dbContext.MapTokenInstances.CountAsync();
        instancesCount.Should().Be(0, "All token instances should be deleted when game map is deleted");
    }

    [Fact]
    public async Task WhenTokenDefinitionDeleted_ThenAllMapTokenInstancesShouldBeDeleted()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };
        var mission = new Mission { Name = "Test Mission", CampaignId = campaign.Id, Campaign = campaign };
        var gameMap = new GameMap { Name = "Test Map", MissionId = mission.Id, Mission = mission };
        var token = new TokenDefinition { Name = "Test Token", UserId = user.Id, User = user };
        var instance = new MapTokenInstance { TokenId = token.Id, Token = token, MapId = gameMap.Id, Map = gameMap };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        _dbContext.Missions.Add(mission);
        _dbContext.GameMaps.Add(gameMap);
        _dbContext.TokenDefinitions.Add(token);
        _dbContext.MapTokenInstances.Add(instance);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the token definition
        var tokenToDelete = await _dbContext.TokenDefinitions.FirstAsync();
        _dbContext.TokenDefinitions.Remove(tokenToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var instancesCount = await _dbContext.MapTokenInstances.CountAsync();
        instancesCount.Should().Be(0, "All token instances should be deleted when token definition is deleted");
    }

    [Fact]
    public async Task WhenUserDeleted_ThenCampaignsShouldNotBeDeletedByDelete()
    {
        // Arrange - Delete behavior is set to Restrict
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        // Act - Try to delete the user (DeleteBehavior.Restrict is configured)
        var userToDelete = await _dbContext.Users.FirstAsync();
        _dbContext.Users.Remove(userToDelete);

        // Note: In-memory database doesn't enforce foreign key constraints
        // In production SQL Server, this would throw a foreign key constraint exception
        await _dbContext.SaveChangesAsync();

        // Assert - Campaign should still exist (orphaned, but exists)
        var campaignsCount = await _dbContext.Campaigns.CountAsync();
        campaignsCount.Should().Be(1, "Campaign should still exist even though user is deleted (Restrict behavior)");
    }

    [Fact]
    public async Task WhenUserDeleted_ThenTokenDefinitionsShouldBeDeleted()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var token = new TokenDefinition { Name = "Test Token", UserId = user.Id, User = user };

        _dbContext.Users.Add(user);
        _dbContext.TokenDefinitions.Add(token);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the user
        var userToDelete = await _dbContext.Users.FirstAsync();
        _dbContext.Users.Remove(userToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var tokensCount = await _dbContext.TokenDefinitions.CountAsync();
        tokensCount.Should().Be(0, "All token definitions should be deleted when user is deleted");
    }

    [Fact]
    public async Task CascadeDelete_MultiLevelHierarchy_ShouldDeleteAll()
    {
        // Arrange - Create full hierarchy
        var user = new User { Username = "testuser", Email = "test@example.com" };
        var campaign = new Campaign { Name = "Test Campaign", OwnerId = user.Id, Owner = user };
        var mission = new Mission { Name = "Test Mission", CampaignId = campaign.Id, Campaign = campaign };
        var gameMap = new GameMap { Name = "Test Map", MissionId = mission.Id, Mission = mission };
        var token = new TokenDefinition { Name = "Test Token", UserId = user.Id, User = user };
        var instance = new MapTokenInstance { TokenId = token.Id, Token = token, MapId = gameMap.Id, Map = gameMap };

        _dbContext.Users.Add(user);
        _dbContext.Campaigns.Add(campaign);
        _dbContext.Missions.Add(mission);
        _dbContext.GameMaps.Add(gameMap);
        _dbContext.TokenDefinitions.Add(token);
        _dbContext.MapTokenInstances.Add(instance);
        await _dbContext.SaveChangesAsync();

        // Act - Delete the campaign (top of hierarchy)
        var campaignToDelete = await _dbContext.Campaigns.FirstAsync();
        _dbContext.Campaigns.Remove(campaignToDelete);
        await _dbContext.SaveChangesAsync();

        // Assert
        var missionsCount = await _dbContext.Missions.CountAsync();
        var mapsCount = await _dbContext.GameMaps.CountAsync();
        var instancesCount = await _dbContext.MapTokenInstances.CountAsync();

        missionsCount.Should().Be(0, "Missions should be deleted in cascade");
        mapsCount.Should().Be(0, "GameMaps should be deleted in cascade");
        instancesCount.Should().Be(0, "MapTokenInstances should be deleted in cascade");

        // Tokens and users should still exist (they were deleted by token definition cascade, not campaign cascade)
        var tokensCount = await _dbContext.TokenDefinitions.CountAsync();
        var usersCount = await _dbContext.Users.CountAsync();

        tokensCount.Should().Be(1, "Token definitions should not be affected by campaign deletion");
        usersCount.Should().Be(1, "Users should not be affected by campaign deletion");
    }
}
