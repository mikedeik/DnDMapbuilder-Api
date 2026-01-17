namespace DnDMapBuilder.Data.Entities;

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
