namespace DnDMapBuilder.Data.Entities;

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
