using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DnDMapBuilder.Infrastructure.Telemetry;

/// <summary>
/// Configures OpenTelemetry for distributed tracing and metrics collection.
/// </summary>
public static class TelemetryConfiguration
{
    /// <summary>
    /// Extends OpenTelemetry tracing configuration with custom instrumentation.
    /// Must be called after AddServiceDefaults() which initializes OpenTelemetry.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        // Extend the existing OpenTelemetry configuration from Aspire ServiceDefaults
        // AddOpenTelemetry() returns the existing builder if already called
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                // Add custom activity sources
                builder
                    .AddSource("DnDMapBuilder.Services")
                    .AddSource("DnDMapBuilder.Repositories");

                // Add SQL Client instrumentation (not included in Aspire defaults)
                builder.AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                });
            });

        // Configure ASP.NET Core instrumentation enrichment
        // This configures the existing instrumentation added by Aspire
        services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = EnrichWithHttpRequest;
            options.EnrichWithHttpResponse = EnrichWithHttpResponse;
        });

        // Configure HTTP Client instrumentation enrichment
        services.Configure<HttpClientTraceInstrumentationOptions>(options =>
        {
            options.RecordException = true;
        });

        // Register telemetry service
        services.AddSingleton<ITelemetryService, TelemetryService>();

        return services;
    }

    private static void EnrichWithHttpRequest(Activity activity, HttpRequest request)
    {
        activity.SetTag("http.request_content_type", request.ContentType);
        activity.SetTag("http.client_ip", request.HttpContext?.Connection?.RemoteIpAddress?.ToString());
    }

    private static void EnrichWithHttpResponse(Activity activity, HttpResponse response)
    {
        activity.SetTag("http.response_content_type", response.ContentType);
    }
}
