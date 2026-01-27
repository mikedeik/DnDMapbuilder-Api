namespace DnDMapBuilder.Data.Entities;

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
    public PublicationStatus PublicationStatus { get; set; } = PublicationStatus.Draft;
    public string MissionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Mission Mission { get; set; } = null!;
    public ICollection<MapTokenInstance> Tokens { get; set; } = new List<MapTokenInstance>();
}
