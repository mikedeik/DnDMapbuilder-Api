namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to create a new token definition.
/// </summary>
public record CreateTokenDefinitionRequest(
    string Name,
    string ImageUrl,
    int Size,
    string Type
);
