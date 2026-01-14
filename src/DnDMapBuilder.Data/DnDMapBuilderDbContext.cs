using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data;

public class DnDMapBuilderDbContext : DbContext
{
    public DnDMapBuilderDbContext(DbContextOptions<DnDMapBuilderDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<Mission> Missions { get; set; } = null!;
    public DbSet<GameMap> GameMaps { get; set; } = null!;
    public DbSet<TokenDefinition> TokenDefinitions { get; set; } = null!;
    public DbSet<MapTokenInstance> MapTokenInstances { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        // Campaign configuration
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Campaigns)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mission configuration
        modelBuilder.Entity<Mission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Missions)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GameMap configuration
        modelBuilder.Entity<GameMap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.GridColor).IsRequired().HasMaxLength(20);
            
            entity.HasOne(e => e.Mission)
                .WithMany(m => m.Maps)
                .HasForeignKey(e => e.MissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TokenDefinition configuration
        modelBuilder.Entity<TokenDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.TokenDefinitions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MapTokenInstance configuration
        modelBuilder.Entity<MapTokenInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Token)
                .WithMany(t => t.MapTokenInstances)
                .HasForeignKey(e => e.TokenId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Map)
                .WithMany(m => m.Tokens)
                .HasForeignKey(e => e.MapId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default admin user
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = "admin-seed-id";
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@dndmapbuilder.com",
                // Password: Admin123!
                PasswordHash = "$2a$11$XxvU8qZ5yP.yxKxQ8zHW7O8qKFdN1LQkGxKvYxGZ.hQvZNzVZY3.S",
                Role = "admin",
                Status = "approved",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
