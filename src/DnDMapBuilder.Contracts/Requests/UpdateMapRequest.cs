namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to update an existing game map.
/// Publication status is managed separately via LiveMapsController.
/// </summary>
public record UpdateMapRequest(
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    List<MapTokenInstanceRequest> Tokens,
    string GridColor,
    double GridOpacity
);
