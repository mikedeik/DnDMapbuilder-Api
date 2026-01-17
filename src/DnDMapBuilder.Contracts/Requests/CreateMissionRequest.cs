namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to create a new mission.
/// </summary>
public record CreateMissionRequest(
    string Name,
    string Description,
    string CampaignId
);
