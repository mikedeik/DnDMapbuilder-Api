using DnDMapBuilder.Infrastructure.HealthChecks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace DnDMapBuilder.UnitTests.Infrastructure.HealthChecks;

public class MemoryHealthCheckTests
{
    private readonly MemoryHealthCheck _healthCheck;

    public MemoryHealthCheckTests()
    {
        _healthCheck = new MemoryHealthCheck();
    }

    [Fact]
    public async Task CheckHealthAsync_WithValidMemory_ReturnsHealthy()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("Memory usage is");
        result.Data.Should().ContainKey("TotalMemoryMB");
        result.Data.Should().ContainKey("MaxMemoryMB");
    }

    [Fact]
    public async Task CheckHealthAsync_WithValidMemory_IncludesMemoryData()
    {
        // Act
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Data.Should().NotBeEmpty();
        result.Data["TotalMemoryMB"].Should().NotBeNull();
        result.Data["MaxMemoryMB"].Should().Be(512);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellationToken_Completes()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _healthCheck.CheckHealthAsync(
            new HealthCheckContext(),
            cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }
}
