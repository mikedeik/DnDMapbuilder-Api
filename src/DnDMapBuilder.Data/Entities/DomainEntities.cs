namespace DnDMapBuilder.Data.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // admin or user
    public string Status { get; set; } = "pending"; // pending, approved, rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public ICollection<TokenDefinition> TokenDefinitions { get; set; } = new List<TokenDefinition>();
}

public class Campaign
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Mission> Missions { get; set; } = new List<Mission>();
}

public class Mission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CampaignId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Campaign Campaign { get; set; } = null!;
    public ICollection<GameMap> Maps { get; set; } = new List<GameMap>();
}

public class GameMap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    // File storage metadata (new fields for multipart upload)
    public string? ImageFileId { get; set; }
    public string? ImageContentType { get; set; }
    public long ImageFileSize { get; set; } = 0;

    public int Rows { get; set; }
    public int Cols { get; set; }
    public string GridColor { get; set; } = "#000000";
    public double GridOpacity { get; set; } = 0.3;
    public string MissionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Mission Mission { get; set; } = null!;
    public ICollection<MapTokenInstance> Tokens { get; set; } = new List<MapTokenInstance>();
}

public class TokenDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    // File storage metadata (new fields for multipart upload)
    public string? ImageFileId { get; set; }
    public string? ImageContentType { get; set; }
    public long ImageFileSize { get; set; } = 0;

    public int Size { get; set; } = 1; // 1, 2, or 3
    public string Type { get; set; } = "player"; // player or enemy
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<MapTokenInstance> MapTokenInstances { get; set; } = new List<MapTokenInstance>();
}

public class MapTokenInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TokenId { get; set; } = string.Empty;
    public string MapId { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public TokenDefinition Token { get; set; } = null!;
    public GameMap Map { get; set; } = null!;
}
