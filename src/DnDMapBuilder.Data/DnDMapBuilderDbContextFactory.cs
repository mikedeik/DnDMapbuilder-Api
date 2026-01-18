using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DnDMapBuilder.Data;

/// <summary>
/// Design-time factory for DnDMapBuilderDbContext.
/// Used by Entity Framework Core for migrations and scaffolding.
/// </summary>
public class DnDMapBuilderDbContextFactory : IDesignTimeDbContextFactory<DnDMapBuilderDbContext>
{
    public DnDMapBuilderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DnDMapBuilderDbContext>();

        // Use the connection string from environment variable, or a default local SQL Server
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
            ?? "Server=localhost;Database=DnDMapBuilder;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;";

        optionsBuilder.UseSqlServer(connectionString);

        return new DnDMapBuilderDbContext(optionsBuilder.Options);
    }
}
