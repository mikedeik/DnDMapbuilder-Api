namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to update an existing campaign.
/// </summary>
public record UpdateCampaignRequest(
    string Name,
    string Description
);
