namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request for a map token instance.
/// </summary>
public record MapTokenInstanceRequest(
    string TokenId,
    int X,
    int Y
);
