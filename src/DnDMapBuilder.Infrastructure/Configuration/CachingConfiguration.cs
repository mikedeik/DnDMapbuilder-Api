using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DnDMapBuilder.Infrastructure.Configuration;

/// <summary>
/// Configuration for response caching policies and cache profiles.
/// </summary>
public static class CachingConfiguration
{
    /// <summary>
    /// Adds response caching services to the dependency injection container.
    /// Configures cache profiles for different endpoint types.
    /// </summary>
    public static IServiceCollection AddResponseCachingConfiguration(this IServiceCollection services)
    {
        services.AddResponseCaching();
        return services;
    }

    /// <summary>
    /// Configures cache profiles for response caching.
    /// Should be called when configuring controllers with AddControllers.
    /// </summary>
    public static IMvcBuilder ConfigureCacheProfiles(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddMvcOptions(options =>
        {
            // Default cache profile: 60 seconds
            options.CacheProfiles.Add("Default60", new()
            {
                Duration = 60,
                Location = ResponseCacheLocation.Any,
                NoStore = false
            });

            // Longer cache profile for static data: 300 seconds (5 minutes)
            options.CacheProfiles.Add("Long300", new()
            {
                Duration = 300,
                Location = ResponseCacheLocation.Any,
                NoStore = false
            });

            // Very short cache for frequently changing data: 10 seconds
            options.CacheProfiles.Add("Short10", new()
            {
                Duration = 10,
                Location = ResponseCacheLocation.Any,
                NoStore = false
            });

            // No cache profile for sensitive operations
            options.CacheProfiles.Add("NoCache", new()
            {
                NoStore = true,
                Location = ResponseCacheLocation.None,
                Duration = 0
            });
        });

        return mvcBuilder;
    }

    /// <summary>
    /// Applies response caching middleware to the application pipeline.
    /// Must be placed before other middleware that depends on caching.
    /// </summary>
    public static IApplicationBuilder UseResponseCachingConfiguration(this IApplicationBuilder app)
    {
        return app.UseResponseCaching();
    }

    /// <summary>
    /// Adds cache control headers for responses.
    /// This middleware ensures proper cache control behavior.
    /// </summary>
    public static IApplicationBuilder UseCacheControlHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // Set cache control header for successful GET requests
            if (context.Request.Method == HttpMethods.Get)
            {
                // Set default cache control if not already set by response cache attribute
                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                {
                    context.Response.Headers["Cache-Control"] = "public, max-age=60";
                }
            }
            else
            {
                // Don't cache non-GET requests
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }

            await next();
        });
    }
}
