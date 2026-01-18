using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DnDMapBuilder.Data;

/// <summary>
/// Utility class for running database migrations and seeding.
/// Can be used by CI/CD pipelines to run migrations before deployment.
/// </summary>
public class MigrationRunner
{
    private readonly DnDMapBuilderDbContext _context;
    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(DnDMapBuilderDbContext context, ILogger<MigrationRunner> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs all pending migrations and seeds the database with initial data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if migrations were applied successfully, false if an error occurred</returns>
    public async Task<bool> MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database migration...");

            // Get pending migrations
            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Found {PendingMigrationCount} pending migrations: {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                // Apply migrations
                await _context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation(" Database migrations completed successfully");
            }
            else
            {
                _logger.LogInformation(" Database is up to date. No migrations to apply.");
            }

            // Initialize database with seed data if needed
            _logger.LogInformation("Initializing database with seed data...");
            await DbInitializer.InitializeAsync(_context);
            _logger.LogInformation(" Database initialization completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database migration");
            return false;
        }
    }

    /// <summary>
    /// Runs all pending migrations without seeding data.
    /// Useful for migration-only operations in CI/CD pipelines.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if migrations were applied successfully, false if an error occurred</returns>
    public async Task<bool> MigrateOnlyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database migration (migration only, no seeding)...");

            // Get pending migrations
            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Found {PendingMigrationCount} pending migrations: {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                // Apply migrations
                await _context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations completed successfully");
            }
            else
            {
                _logger.LogInformation("Database is up to date. No migrations to apply.");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database migration");
            return false;
        }
    }

    /// <summary>
    /// Gets information about the current database state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<DatabaseInfo> GetDatabaseInfoAsync(CancellationToken cancellationToken = default)
    {
        var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        return new DatabaseInfo
        {
            AppliedMigrationsCount = appliedMigrations.Count,
            AppliedMigrations = appliedMigrations,
            PendingMigrationsCount = pendingMigrations.Count,
            PendingMigrations = pendingMigrations,
            IsDatabaseCreated = await _context.Database.CanConnectAsync(cancellationToken)
        };
    }
}

/// <summary>
/// Information about the current database state.
/// </summary>
public class DatabaseInfo
{
    public int AppliedMigrationsCount { get; set; }
    public List<string> AppliedMigrations { get; set; } = new();
    public int PendingMigrationsCount { get; set; }
    public List<string> PendingMigrations { get; set; } = new();
    public bool IsDatabaseCreated { get; set; }

    public override string ToString()
    {
        return $"Database Info:\n" +
               $"  - Is Created: {IsDatabaseCreated}\n" +
               $"  - Applied Migrations: {AppliedMigrationsCount}\n" +
               $"    {string.Join("\n    ", AppliedMigrations.Select(m => $"{m}"))}\n" +
               $"  - Pending Migrations: {PendingMigrationsCount}\n" +
               $"    {string.Join("\n    ", PendingMigrations.Select(m => $"{m}"))}";
    }
}
