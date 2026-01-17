using DnDMapBuilder.Data.Entities;
using FluentAssertions;
using Xunit;

namespace DnDMapBuilder.UnitTests.Entities;

public class UserEntityTests
{
    [Fact]
    public void User_Initialization_ShouldSetDefaults()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().NotBeNullOrEmpty();
        user.Username.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.PasswordHash.Should().Be(string.Empty);
        user.Role.Should().Be("user");
        user.Status.Should().Be("pending");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        user.Campaigns.Should().BeEmpty();
        user.TokenDefinitions.Should().BeEmpty();
    }

    [Fact]
    public void User_ShouldHaveUniqueIds()
    {
        // Act
        var user1 = new User();
        var user2 = new User();

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }
}

public class CampaignEntityTests
{
    [Fact]
    public void Campaign_Initialization_ShouldSetDefaults()
    {
        // Act
        var campaign = new Campaign();

        // Assert
        campaign.Id.Should().NotBeNullOrEmpty();
        campaign.Name.Should().Be(string.Empty);
        campaign.Description.Should().Be(string.Empty);
        campaign.OwnerId.Should().Be(string.Empty);
        campaign.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        campaign.Missions.Should().BeEmpty();
    }

    [Fact]
    public void Campaign_WithProperties_ShouldHoldValues()
    {
        // Arrange
        var ownerId = "user123";
        var name = "Dragon Quest";
        var description = "A grand adventure";

        // Act
        var campaign = new Campaign
        {
            OwnerId = ownerId,
            Name = name,
            Description = description
        };

        // Assert
        campaign.OwnerId.Should().Be(ownerId);
        campaign.Name.Should().Be(name);
        campaign.Description.Should().Be(description);
    }
}

public class MissionEntityTests
{
    [Fact]
    public void Mission_Initialization_ShouldSetDefaults()
    {
        // Act
        var mission = new Mission();

        // Assert
        mission.Id.Should().NotBeNullOrEmpty();
        mission.Name.Should().Be(string.Empty);
        mission.Description.Should().Be(string.Empty);
        mission.CampaignId.Should().Be(string.Empty);
        mission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        mission.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        mission.Maps.Should().BeEmpty();
    }
}

public class GameMapEntityTests
{
    [Fact]
    public void GameMap_Initialization_ShouldSetDefaults()
    {
        // Act
        var map = new GameMap();

        // Assert
        map.Id.Should().NotBeNullOrEmpty();
        map.Name.Should().Be(string.Empty);
        map.ImageUrl.Should().BeNull();
        map.GridColor.Should().Be("#000000");
        map.GridOpacity.Should().Be(0.3);
        map.ImageFileSize.Should().Be(0);
        map.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        map.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        map.Tokens.Should().BeEmpty();
    }

    [Fact]
    public void GameMap_WithProperties_ShouldHoldValues()
    {
        // Arrange
        var name = "Tavern";
        var rows = 10;
        var cols = 15;

        // Act
        var map = new GameMap
        {
            Name = name,
            Rows = rows,
            Cols = cols
        };

        // Assert
        map.Name.Should().Be(name);
        map.Rows.Should().Be(rows);
        map.Cols.Should().Be(cols);
    }
}

public class TokenDefinitionEntityTests
{
    [Fact]
    public void TokenDefinition_Initialization_ShouldSetDefaults()
    {
        // Act
        var token = new TokenDefinition();

        // Assert
        token.Id.Should().NotBeNullOrEmpty();
        token.Name.Should().Be(string.Empty);
        token.ImageUrl.Should().Be(string.Empty);
        token.Size.Should().Be(1);
        token.Type.Should().Be("player");
        token.UserId.Should().Be(string.Empty);
        token.ImageFileSize.Should().Be(0);
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        token.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        token.MapTokenInstances.Should().BeEmpty();
    }

    [Fact]
    public void TokenDefinition_WithProperties_ShouldHoldValues()
    {
        // Arrange
        var name = "Knight";
        var type = "player";
        var size = 2;

        // Act
        var token = new TokenDefinition
        {
            Name = name,
            Type = type,
            Size = size
        };

        // Assert
        token.Name.Should().Be(name);
        token.Type.Should().Be(type);
        token.Size.Should().Be(size);
    }
}

public class MapTokenInstanceEntityTests
{
    [Fact]
    public void MapTokenInstance_Initialization_ShouldSetDefaults()
    {
        // Act
        var instance = new MapTokenInstance();

        // Assert
        instance.Id.Should().NotBeNullOrEmpty();
        instance.TokenId.Should().Be(string.Empty);
        instance.MapId.Should().Be(string.Empty);
        instance.X.Should().Be(0);
        instance.Y.Should().Be(0);
        instance.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MapTokenInstance_WithPosition_ShouldHoldValues()
    {
        // Arrange
        const int x = 5;
        const int y = 10;
        var tokenId = "token123";
        var mapId = "map123";

        // Act
        var instance = new MapTokenInstance
        {
            X = x,
            Y = y,
            TokenId = tokenId,
            MapId = mapId
        };

        // Assert
        instance.X.Should().Be(x);
        instance.Y.Should().Be(y);
        instance.TokenId.Should().Be(tokenId);
        instance.MapId.Should().Be(mapId);
    }
}
