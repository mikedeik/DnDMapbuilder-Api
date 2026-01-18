namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to create a new campaign.
/// </summary>
public record CreateCampaignRequest(
    string Name,
    string Description
);
