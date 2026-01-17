using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace DnDMapBuilder.Infrastructure.Configuration;

/// <summary>
/// Configuration for rate limiting policies in the API.
/// </summary>
public static class RateLimitConfiguration
{
    /// <summary>
    /// Adds rate limiting services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Anonymous users: 100 requests per minute
            options.AddPolicy("anonymous", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Authenticated users: 300 requests per minute
            options.AddPolicy("authenticated", httpContext =>
            {
                var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // File uploads: 10 requests per minute
            options.AddPolicy("fileUpload", httpContext =>
            {
                var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"{userId}-upload",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // Return 429 on rate limit exceeded
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                context.HttpContext.Response.ContentType = "application/json";

                var response = new { success = false, message = "Rate limit exceeded. Please try again later.", retryAfter = 60 };
                await context.HttpContext.Response.WriteAsJsonAsync(response);
            };
        });

        return services;
    }

    /// <summary>
    /// Applies rate limiting middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseRateLimitingConfiguration(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}
