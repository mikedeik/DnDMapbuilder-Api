using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace DnDMapBuilder.Infrastructure.HealthChecks;

/// <summary>
/// Configures health checks for the application.
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Adds health checks to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var builder = services
            .AddHealthChecks()
            .AddSqlServer(
                connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"),
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "ready", "live" })
            .AddCheck("memory_check", new MemoryHealthCheck(), HealthStatus.Degraded, tags: new[] { "ready" });

        return services;
    }

    /// <summary>
    /// Maps health check endpoints to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder</param>
    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        // General health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = WriteHealthCheckResponse
        });

        // Readiness probe for Kubernetes (checks all required services)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = WriteHealthCheckResponse,
            Predicate = registration => registration.Tags.Contains("ready")
        });

        // Liveness probe for Kubernetes (checks if service is running)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = WriteHealthCheckResponse,
            Predicate = registration => registration.Tags.Contains("live")
        });
    }

    private static async Task WriteHealthCheckResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(kvp => new
            {
                name = kvp.Key,
                status = kvp.Value.Status.ToString(),
                duration = kvp.Value.Duration.TotalMilliseconds,
                description = kvp.Value.Description ?? string.Empty,
                data = kvp.Value.Data.Count > 0 ? kvp.Value.Data : null
            })
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        await context.Response.WriteAsJsonAsync(response, options);
    }
}

/// <summary>
/// Health check for available memory.
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private const long MaxMemoryUsageMb = 512; // Adjust based on your requirements

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var totalMemory = GC.GetTotalMemory(false);
        var memoryUsageMb = totalMemory / 1024 / 1024;

        var data = new Dictionary<string, object>
        {
            { "TotalMemoryMB", memoryUsageMb },
            { "MaxMemoryMB", MaxMemoryUsageMb }
        };

        if (memoryUsageMb > MaxMemoryUsageMb)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Memory usage is {memoryUsageMb}MB which exceeds the threshold of {MaxMemoryUsageMb}MB",
                    data: data));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                $"Memory usage is {memoryUsageMb}MB",
                data: data));
    }
}
