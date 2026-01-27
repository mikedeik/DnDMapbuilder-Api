using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Events;
using DnDMapBuilder.Contracts.Interfaces;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using PublicationStatusEntity = DnDMapBuilder.Data.Entities.PublicationStatus;
using PublicationStatusDto = DnDMapBuilder.Contracts.DTOs.PublicationStatus;

namespace DnDMapBuilder.UnitTests.Services;

/// <summary>
/// Unit tests for LiveMapService covering all broadcast methods and authorization.
/// </summary>
public class LiveMapServiceTests
{
    private readonly Mock<IGameMapRepository> _mockMapRepository;
    private readonly Mock<IGameMapHub> _mockHubContext;
    private readonly Mock<IGameMapService> _mockGameMapService;
    private readonly Mock<ILogger<LiveMapService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly LiveMapService _service;

    public LiveMapServiceTests()
    {
        _mockMapRepository = new Mock<IGameMapRepository>();
        _mockHubContext = new Mock<IGameMapHub>();
        _mockGameMapService = new Mock<IGameMapService>();
        _mockLogger = new Mock<ILogger<LiveMapService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Default throttle window configuration
        _mockConfiguration.Setup(x => x["LiveMap:ThrottleWindowMs"]).Returns("100");

        _service = new LiveMapService(
            _mockMapRepository.Object,
            _mockHubContext.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockGameMapService.Object);
    }

    private GameMap CreateLiveMap(string id = "map1", string name = "Test Map")
    {
        return new GameMap
        {
            Id = id,
            Name = name,
            Rows = 10,
            Cols = 10,
            GridColor = "#000000",
            GridOpacity = 0.3,
            ImageUrl = null,
            MissionId = "mission1",
            PublicationStatus = PublicationStatusEntity.Live,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tokens = new List<MapTokenInstance>()
        };
    }

    private GameMap CreateDraftMap(string id = "map2", string name = "Draft Map")
    {
        var map = CreateLiveMap(id, name);
        map.PublicationStatus = PublicationStatusEntity.Draft;
        return map;
    }

    #region BroadcastMapUpdateAsync Tests

    [Fact]
    public async Task BroadcastMapUpdateAsync_WithLiveMap_CallsHubSendAsync()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastMapUpdateAsync("map1");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "MapUpdated", It.IsAny<MapUpdatedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastMapUpdateAsync_WithDraftMap_DoesNotCallHubSendAsync()
    {
        // Arrange
        var map = CreateDraftMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map2", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastMapUpdateAsync("map2");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default),
            Times.Never);
    }

    [Fact]
    public async Task BroadcastMapUpdateAsync_WithNullMap_LogsWarningAndReturns()
    {
        // Arrange
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("nonexistent", default))
            .ReturnsAsync((GameMap?)null);

        // Act
        await _service.BroadcastMapUpdateAsync("nonexistent");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default),
            Times.Never);
    }

    [Fact]
    public async Task BroadcastMapUpdateAsync_SendsEventWithCorrectData()
    {
        // Arrange
        var map = CreateLiveMap("map1", "Test Map");
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        MapUpdatedEvent? capturedEvent = null;
        _mockHubContext.Setup(h => h.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MapUpdatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, object?, CancellationToken>((g, m, e, ct) =>
            {
                capturedEvent = e as MapUpdatedEvent;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.BroadcastMapUpdateAsync("map1");

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.MapId.Should().Be("map1");
        capturedEvent.Name.Should().Be("Test Map");
        capturedEvent.Rows.Should().Be(10);
        capturedEvent.Cols.Should().Be(10);
    }

    #endregion

    #region BroadcastTokenMovedAsync Tests

    [Fact]
    public async Task BroadcastTokenMovedAsync_WithLiveMap_CallsHubSendAsync()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastTokenMovedAsync("map1", "token1", 5, 5);

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenMoved", It.IsAny<TokenMovedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastTokenMovedAsync_WithDraftMap_DoesNotBroadcast()
    {
        // Arrange
        var map = CreateDraftMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map2", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastTokenMovedAsync("map2", "token1", 5, 5);

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default),
            Times.Never);
    }

    #endregion

    #region BroadcastTokenAddedAsync Tests

    [Fact]
    public async Task BroadcastTokenAddedAsync_WithLiveMapAndValidToken_CallsHubSendAsync()
    {
        // Arrange
        var map = CreateLiveMap();
        var token = new MapTokenInstance
        {
            Id = "token1",
            TokenId = "def1",
            X = 5,
            Y = 5,
            CreatedAt = DateTime.UtcNow
        };
        map.Tokens.Add(token);

        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastTokenAddedAsync("map1", "token1");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenAdded", It.IsAny<TokenAddedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastTokenAddedAsync_WithNonexistentToken_LogsWarning()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastTokenAddedAsync("map1", "nonexistent");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default),
            Times.Never);
    }

    #endregion

    #region BroadcastTokenRemovedAsync Tests

    [Fact]
    public async Task BroadcastTokenRemovedAsync_WithLiveMap_CallsHubSendAsync()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act
        await _service.BroadcastTokenRemovedAsync("map1", "token1");

        // Assert
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenRemoved", It.IsAny<TokenRemovedEvent>(), default),
            Times.Once);
    }

    #endregion

    #region SetMapPublicationStatusAsync Tests

    [Fact]
    public async Task SetMapPublicationStatusAsync_WithAuthorizedUser_UpdatesAndBroadcasts()
    {
        // Arrange
        var userId = "user1";
        var map = CreateDraftMap("map1");
        var existingMapDto = new GameMapDto(
            "map1", "Test Map", null, 10, 10,
            new List<MapTokenInstanceDto>(),
            "#000000", 0.3, "mission1", PublicationStatusDto.Draft);

        _mockGameMapService.Setup(s => s.GetByIdAsync("map1", userId, default))
            .ReturnsAsync(existingMapDto);
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);
        _mockMapRepository.Setup(r => r.UpdateAsync(It.IsAny<GameMap>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetMapPublicationStatusAsync("map1", PublicationStatusDto.Live, userId);

        // Assert
        map.PublicationStatus.Should().Be(PublicationStatusEntity.Live);
        _mockMapRepository.Verify(r => r.UpdateAsync(It.IsAny<GameMap>(), default), Times.Once);
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "MapStatusChanged", It.IsAny<MapStatusChangedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task SetMapPublicationStatusAsync_WithUnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = "user1";
        _mockGameMapService.Setup(s => s.GetByIdAsync("map1", userId, default))
            .ReturnsAsync((GameMapDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.SetMapPublicationStatusAsync("map1", PublicationStatusDto.Live, userId));
    }

    #endregion

    #region GetMapStateSnapshotAsync Tests

    [Fact]
    public async Task GetMapStateSnapshotAsync_WithLiveMap_ReturnsSnapshot()
    {
        // Arrange
        var userId = "user1";
        var mapDto = new GameMapDto(
            "map1", "Test Map", null, 10, 10,
            new List<MapTokenInstanceDto>(),
            "#000000", 0.3, "mission1", PublicationStatusDto.Live);

        _mockGameMapService.Setup(s => s.GetByIdAsync("map1", userId, default))
            .ReturnsAsync(mapDto);

        // Act
        var snapshot = await _service.GetMapStateSnapshotAsync("map1", userId);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot!.Map.Should().Be(mapDto);
        snapshot.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetMapStateSnapshotAsync_WithDraftMap_ReturnsNull()
    {
        // Arrange
        var userId = "user1";
        var mapDto = new GameMapDto(
            "map2", "Draft Map", null, 10, 10,
            new List<MapTokenInstanceDto>(),
            "#000000", 0.3, "mission1", PublicationStatusDto.Draft);

        _mockGameMapService.Setup(s => s.GetByIdAsync("map2", userId, default))
            .ReturnsAsync(mapDto);

        // Act
        var snapshot = await _service.GetMapStateSnapshotAsync("map2", userId);

        // Assert
        snapshot.Should().BeNull();
    }

    [Fact]
    public async Task GetMapStateSnapshotAsync_WithUnauthorizedUser_ReturnsNull()
    {
        // Arrange
        var userId = "user1";
        _mockGameMapService.Setup(s => s.GetByIdAsync("map1", userId, default))
            .ReturnsAsync((GameMapDto?)null);

        // Act
        var snapshot = await _service.GetMapStateSnapshotAsync("map1", userId);

        // Assert
        snapshot.Should().BeNull();
    }

    #endregion

    #region Throttling Tests

    [Fact]
    public async Task BroadcastTokenMovedAsync_RapidConsecutiveCalls_ThrottlesAfterFirstBroadcast()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Act - Call broadcast multiple times in quick succession
        await _service.BroadcastTokenMovedAsync("map1", "token1", 0, 0);
        await _service.BroadcastTokenMovedAsync("map1", "token1", 1, 1);
        await _service.BroadcastTokenMovedAsync("map1", "token1", 2, 2);

        // Assert - Should only broadcast once due to throttling
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenMoved", It.IsAny<TokenMovedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastTokenMovedAsync_DifferentMaps_HaveIndependentThrottle()
    {
        // Arrange
        var map1 = CreateLiveMap("map1");
        var map2 = CreateLiveMap("map2");
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map1);
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map2", default))
            .ReturnsAsync(map2);

        // Act - Broadcast on different maps
        await _service.BroadcastTokenMovedAsync("map1", "token1", 0, 0);
        await _service.BroadcastTokenMovedAsync("map2", "token1", 0, 0);

        // Assert - Both should broadcast (different maps have independent throttle)
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenMoved", It.IsAny<TokenMovedEvent>(), default),
            Times.Once);
        _mockHubContext.Verify(
            h => h.SendAsync("map_map2", "TokenMoved", It.IsAny<TokenMovedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastTokenMovedAsync_AfterThrottleWindowElapsed_AllowsBroadcast()
    {
        // Arrange
        var map = CreateLiveMap();
        _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default))
            .ReturnsAsync(map);

        // Use shorter throttle window for testing (10ms)
        _mockConfiguration.Setup(x => x["LiveMap:ThrottleWindowMs"]).Returns("10");
        var serviceWithShortThrottle = new LiveMapService(
            _mockMapRepository.Object,
            _mockHubContext.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockGameMapService.Object);

        // Act - First broadcast
        await serviceWithShortThrottle.BroadcastTokenMovedAsync("map1", "token1", 0, 0);

        // Wait for throttle window to elapse
        await Task.Delay(15);

        // Second broadcast after throttle window
        await serviceWithShortThrottle.BroadcastTokenMovedAsync("map1", "token1", 1, 1);

        // Assert - Both broadcasts should go through
        _mockHubContext.Verify(
            h => h.SendAsync("map_map1", "TokenMoved", It.IsAny<TokenMovedEvent>(), default),
            Times.Exactly(2));
    }

    #endregion

}
