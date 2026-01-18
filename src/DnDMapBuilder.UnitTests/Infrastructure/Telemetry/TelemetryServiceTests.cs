using DnDMapBuilder.Infrastructure.Telemetry;
using FluentAssertions;
using Xunit;

namespace DnDMapBuilder.UnitTests.Infrastructure.Telemetry;

public class TelemetryServiceTests
{
    private readonly TelemetryService _telemetryService;

    public TelemetryServiceTests()
    {
        _telemetryService = new TelemetryService();
    }

    [Fact]
    public void RecordAuthenticationAttempt_WithSuccessfulAuth_ShouldRecord()
    {
        // Arrange
        var userId = 123;
        var success = true;
        var durationMs = 150L;

        // Act
        _telemetryService.RecordAuthenticationAttempt(userId, success, durationMs);

        // Assert
        // Telemetry records are internal and cannot be directly verified, but the method should not throw
        // This test primarily ensures no exceptions occur
    }

    [Fact]
    public void RecordAuthenticationAttempt_WithFailedAuth_ShouldRecord()
    {
        // Arrange
        const long durationMs = 100L;

        // Act
        _telemetryService.RecordAuthenticationAttempt(null, false, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Theory]
    [InlineData("issue")]
    [InlineData("refresh")]
    [InlineData("revoke")]
    public void RecordTokenOperation_WithValidOperationType_ShouldRecord(string operationType)
    {
        // Act
        _telemetryService.RecordTokenOperation(operationType, true);

        // Assert
        // Should not throw any exceptions
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public void RecordCampaignOperation_WithValidOperationType_ShouldRecord(string operationType)
    {
        // Arrange
        const int campaignId = 1;
        const long durationMs = 200L;

        // Act
        _telemetryService.RecordCampaignOperation(operationType, campaignId, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public void RecordMissionOperation_WithValidOperationType_ShouldRecord(string operationType)
    {
        // Arrange
        const int missionId = 1;
        const long durationMs = 200L;

        // Act
        _telemetryService.RecordMissionOperation(operationType, missionId, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public void RecordMapOperation_WithValidOperationType_ShouldRecord(string operationType)
    {
        // Arrange
        const int mapId = 1;
        const long durationMs = 200L;

        // Act
        _telemetryService.RecordMapOperation(operationType, mapId, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public void RecordTokenInstanceOperation_WithValidOperationType_ShouldRecord(string operationType)
    {
        // Arrange
        const int tokenId = 1;
        const long durationMs = 150L;

        // Act
        _telemetryService.RecordTokenInstanceOperation(operationType, tokenId, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Fact]
    public void RecordFileUpload_WithSuccessfulUpload_ShouldRecord()
    {
        // Arrange
        const string fileName = "test.png";
        const long fileSizeBytes = 1024 * 100; // 100KB
        const bool success = true;
        const long durationMs = 500L;

        // Act
        _telemetryService.RecordFileUpload(fileName, fileSizeBytes, success, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Fact]
    public void RecordFileUpload_WithFailedUpload_ShouldRecord()
    {
        // Arrange
        const string fileName = "test.png";
        const long fileSizeBytes = 0;
        const bool success = false;
        const long durationMs = 100L;

        // Act
        _telemetryService.RecordFileUpload(fileName, fileSizeBytes, success, durationMs);

        // Assert
        // Should not throw any exceptions
    }

    [Fact]
    public void StartActivity_WithValidActivityName_ShouldReturnActivityOrNull()
    {
        // Act
        var activity = _telemetryService.StartActivity("test_activity");

        // Assert
        // Activity might be null if OpenTelemetry is not configured, which is valid in this context
        if (activity != null)
        {
            activity.DisplayName.Should().Be("test_activity");
            activity.Dispose();
        }
    }

    [Fact]
    public void RecordAuthenticationAttempt_WithMultipleAttempts_ShouldRecordAll()
    {
        // Act
        for (int i = 0; i < 5; i++)
        {
            _telemetryService.RecordAuthenticationAttempt(null, false, 100L);
        }

        // Assert
        // Should not throw any exceptions and should handle multiple calls gracefully
    }

    [Fact]
    public void RecordCampaignOperation_WithMixedOperations_ShouldRecord()
    {
        // Act
        _telemetryService.RecordCampaignOperation("create", 1, 100L);
        _telemetryService.RecordCampaignOperation("update", 1, 150L);
        _telemetryService.RecordCampaignOperation("delete", 1, 200L);

        // Assert
        // Should not throw any exceptions
    }

    [Fact]
    public void RecordTokenOperation_WithCaseInsensitivity_ShouldRecord()
    {
        // Act
        _telemetryService.RecordTokenOperation("ISSUE", true);
        _telemetryService.RecordTokenOperation("Refresh", true);
        _telemetryService.RecordTokenOperation("REVOKE", false);

        // Assert
        // Should not throw any exceptions regardless of case
    }
}
