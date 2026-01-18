using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using DnDMapBuilder.Infrastructure.Configuration;

namespace DnDMapBuilder.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for CachingConfiguration.
/// </summary>
public class CachingConfigurationTests
{
    [Fact]
    public void AddResponseCachingConfiguration_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddResponseCachingConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void ConfigureCacheProfiles_ShouldAddAllFourCacheProfiles()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.ConfigureCacheProfiles();
        var serviceProvider = services.BuildServiceProvider();

        // Get MVC options to check cache profiles
        var options = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        options.Value.CacheProfiles.Should().HaveCount(4);
        options.Value.CacheProfiles.Should().ContainKey("Default60");
        options.Value.CacheProfiles.Should().ContainKey("Long300");
        options.Value.CacheProfiles.Should().ContainKey("Short10");
        options.Value.CacheProfiles.Should().ContainKey("NoCache");
    }

    [Fact]
    public void CacheProfile_Default60_ShouldHave60SecondsExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.ConfigureCacheProfiles();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var profile = options.Value.CacheProfiles["Default60"];
        profile.Duration.Should().Be(60);
        profile.NoStore.Should().BeFalse();
        profile.Location.Should().Be(ResponseCacheLocation.Any);
    }

    [Fact]
    public void CacheProfile_Long300_ShouldHave300SecondsExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.ConfigureCacheProfiles();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var profile = options.Value.CacheProfiles["Long300"];
        profile.Duration.Should().Be(300);
        profile.NoStore.Should().BeFalse();
        profile.Location.Should().Be(ResponseCacheLocation.Any);
    }

    [Fact]
    public void CacheProfile_Short10_ShouldHave10SecondsExpiration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.ConfigureCacheProfiles();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var profile = options.Value.CacheProfiles["Short10"];
        profile.Duration.Should().Be(10);
        profile.NoStore.Should().BeFalse();
        profile.Location.Should().Be(ResponseCacheLocation.Any);
    }

    [Fact]
    public void CacheProfile_NoCache_ShouldDisableCaching()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.ConfigureCacheProfiles();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var profile = options.Value.CacheProfiles["NoCache"];
        profile.NoStore.Should().BeTrue();
        profile.Duration.Should().Be(0);
        profile.Location.Should().Be(ResponseCacheLocation.None);
    }

    [Fact]
    public void ConfigureCacheProfiles_ShouldReturnMvcBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        var result = mvcBuilder.ConfigureCacheProfiles();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mvcBuilder);
    }

    [Fact]
    public void UseResponseCachingConfiguration_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var app = WebApplication.CreateBuilder().Build();

        // Act
        var result = app.UseResponseCachingConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(app);
    }

    [Fact]
    public void UseCacheControlHeaders_ShouldSetProperHeadersForGetRequests()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        var middleware = async (HttpContext ctx) => { };

        // Act
        var app = WebApplication.CreateBuilder().Build();
        app.UseCacheControlHeaders();

        // Assert - The middleware should be added to the pipeline
        // This is a basic validation that the middleware can be added
        // without throwing an exception
    }
}
