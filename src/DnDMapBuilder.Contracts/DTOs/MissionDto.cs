namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a mission.
/// </summary>
public record MissionDto(
    string Id,
    string Name,
    string Description,
    List<GameMapDto> Maps,
    string CampaignId
);
