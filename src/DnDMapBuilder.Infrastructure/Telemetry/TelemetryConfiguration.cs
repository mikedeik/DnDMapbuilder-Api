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
    /// Adds OpenTelemetry tracing and metrics to the service collection.
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
        var otlpEndpoint = configuration["Telemetry:OtlpEndpoint"];

        // Configure tracing
        services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            builder
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = EnrichWithHttpRequest;
                    options.EnrichWithHttpResponse = EnrichWithHttpResponse;
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSource("DnDMapBuilder.Services")
                .AddSource("DnDMapBuilder.Repositories");

            // Export to OTLP endpoint if configured
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                builder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            }
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
