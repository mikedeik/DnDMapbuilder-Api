using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DnDMapBuilder.Infrastructure.Logging;

/// <summary>
/// Configures Serilog structured logging for the application.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog logger with appropriate sinks and enrichers.
    /// </summary>
    /// <param name="environment">The application environment (Development, Staging, Production)</param>
    /// <param name="isDevelopment">Whether running in development mode</param>
    /// <returns>Configured Serilog ILogger instance</returns>
    public static Serilog.ILogger ConfigureLogging(string? environment, bool isDevelopment)
    {
        var logLevel = isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information;

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithThreadId();

        // Override logging levels for specific namespaces
        loggerConfig
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);

        // Console sink for all environments
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        // File sink with rolling files
        loggerConfig.WriteTo.File(
            path: "logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

        // Seq sink for structured log aggregation (optional, for development/staging)
        var seqUrl = Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
        if (!string.IsNullOrEmpty(seqUrl))
        {
            loggerConfig.WriteTo.Seq(seqUrl);
        }

        return loggerConfig.CreateLogger();
    }

}
