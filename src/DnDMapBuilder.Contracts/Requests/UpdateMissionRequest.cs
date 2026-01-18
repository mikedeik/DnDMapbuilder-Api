namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to update an existing mission.
/// </summary>
public record UpdateMissionRequest(
    string Name,
    string Description
);
