namespace DnDMapBuilder.Data.Entities;

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
