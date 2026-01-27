namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to create a new game map.
/// Maps are always created with Draft status and can be published via LiveMapsController.
/// </summary>
public record CreateMapRequest(
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    string GridColor,
    double GridOpacity,
    string MissionId
);
