using DnDMapBuilder.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DnDMapBuilder.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(DnDMapBuilderDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if admin user already exists
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? throw new InvalidOperationException("ADMIN_EMAIL environment variable is required");

        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD")
            ?? throw new InvalidOperationException("ADMIN_DEFAULT_PASSWORD environment variable is required");

        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Role == "admin" && u.Email == adminEmail);

        if (existingAdmin == null)
        {
            // Create new admin user
            var admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, 11),
                Role = "admin",
                Status = "approved",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine($"Admin user created: {adminEmail}");
        }
        else
        {
            // Update existing admin user's password
            existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, 11);
            existingAdmin.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            Console.WriteLine($"Admin user password updated: {adminEmail}");
        }
    }
}
