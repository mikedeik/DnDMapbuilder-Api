using System.Reflection;
using Xunit;
using DnDMapBuilder.Api.Controllers;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Data;
using DnDMapBuilder.Data.Repositories;

namespace DnDMapBuilder.ArchitectureTests;

/// <summary>
/// Architecture tests to enforce layered architecture and design principles.
/// Ensures no circular dependencies and proper separation of concerns.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly ApiAssembly = typeof(AuthController).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IAuthService).Assembly;
    private static readonly Assembly DataAssembly = typeof(DnDMapBuilderDbContext).Assembly;

    /// <summary>
    /// All controllers should inherit from ControllerBase.
    /// </summary>
    [Fact]
    public void All_Controllers_ShouldInheritFrom_ControllerBase()
    {
        var controllerTypes = ApiAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Api.Controllers") == true &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.IsCompilerGenerated())
            .ToList();

        var invalidControllers = controllerTypes
            .Where(t => !typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t))
            .ToList();

        Assert.Empty(invalidControllers);
    }

    /// <summary>
    /// All controller names should end with "Controller".
    /// </summary>
    [Fact]
    public void All_Controllers_ShouldHaveName_EndingWith_Controller()
    {
        var controllerTypes = ApiAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Api.Controllers") == true &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.IsCompilerGenerated())
            .ToList();

        var invalidControllers = controllerTypes.Where(c => !c.Name.EndsWith("Controller")).ToList();
        Assert.Empty(invalidControllers);
    }

    /// <summary>
    /// Application layer should not reference API layer (no circular dependency).
    /// </summary>
    [Fact]
    public void ApplicationLayer_ShouldNotReference_ApiLayer()
    {
        var appTypes = ApplicationAssembly.GetTypes();
        var apiNamespace = "DnDMapBuilder.Api";

        var appWithApiDeps = appTypes
            .Where(t => t.GetProperties()
                .Any(p => p.PropertyType.Namespace?.StartsWith(apiNamespace) == true) ||
                   t.GetFields()
                .Any(f => f.FieldType.Namespace?.StartsWith(apiNamespace) == true))
            .ToList();

        Assert.Empty(appWithApiDeps);
    }

    /// <summary>
    /// Data layer should not reference Application layer (no circular dependency).
    /// </summary>
    [Fact]
    public void DataLayer_ShouldNotReference_ApplicationLayer()
    {
        var dataTypes = DataAssembly.GetTypes();
        var appNamespace = "DnDMapBuilder.Application";

        var dataWithAppDeps = dataTypes
            .Where(t => t.GetProperties()
                .Any(p => p.PropertyType.Namespace?.StartsWith(appNamespace) == true) ||
                   t.GetFields()
                .Any(f => f.FieldType.Namespace?.StartsWith(appNamespace) == true))
            .ToList();

        Assert.Empty(dataWithAppDeps);
    }

    /// <summary>
    /// All interfaces in Application layer should be implemented by services.
    /// </summary>
    [Fact]
    public void All_ServiceInterfaces_ShouldHaveImplementation()
    {
        var interfaceTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Application.Interfaces") == true &&
                        t.IsInterface)
            .ToList();

        var serviceTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Application.Services") == true &&
                        t.IsClass &&
                        !t.IsAbstract)
            .ToList();

        foreach (var interfaceType in interfaceTypes)
        {
            var implementingService = serviceTypes.FirstOrDefault(s => interfaceType.IsAssignableFrom(s));
            Assert.NotNull(implementingService);
        }
    }

    /// <summary>
    /// All repositories should have names ending with "Repository".
    /// </summary>
    [Fact]
    public void All_Repositories_ShouldHaveName_EndingWith_Repository()
    {
        var repositoryTypes = DataAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Data.Repositories") == true &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.EndsWith("Tests") &&
                        !t.IsCompilerGenerated())
            .ToList();

        // Generic classes like GenericRepository<T> have names like "GenericRepository`1"
        // Remove the generic type parameter indicators for name validation
        var invalidRepos = repositoryTypes.Where(r =>
        {
            var cleanName = r.Name.Split('`')[0]; // Remove generic type parameter count
            return !cleanName.EndsWith("Repository");
        }).ToList();
        Assert.Empty(invalidRepos);
    }

    /// <summary>
    /// Services should not directly depend on DbContext (use repositories instead).
    /// </summary>
    [Fact]
    public void Services_ShouldNotHaveDirect_DbContextDependency()
    {
        var servicesWithDbContext = ApplicationAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith("DnDMapBuilder.Application.Services") == true &&
                        t.IsClass &&
                        !t.IsAbstract)
            .Where(t => t.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType.Name == "DnDMapBuilderDbContext"))
            .ToList();

        // Services should use repositories, not DbContext directly
        Assert.Empty(servicesWithDbContext);
    }

    /// <summary>
    /// Check that data layer doesn't reference API layer directly.
    /// </summary>
    [Fact]
    public void DataLayer_ShouldNotReference_ApiLayer()
    {
        var dataTypes = DataAssembly.GetTypes();
        var apiNamespace = "DnDMapBuilder.Api";

        var dataWithApiDeps = dataTypes
            .Where(t => t.GetProperties()
                .Any(p => p.PropertyType.Namespace?.StartsWith(apiNamespace) == true) ||
                   t.GetFields()
                .Any(f => f.FieldType.Namespace?.StartsWith(apiNamespace) == true))
            .ToList();

        Assert.Empty(dataWithApiDeps);
    }
}

/// <summary>
/// Internal extensions for architecture tests.
/// </summary>
internal static class ArchitectureTestExtensions
{
    public static bool IsCompilerGenerated(this Type type)
    {
        return type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null;
    }
}
