namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a campaign.
/// </summary>
public record CampaignDto(
    string Id,
    string Name,
    string Description,
    List<MissionDto> Missions,
    string OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
