namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to update an existing token definition.
/// </summary>
public record UpdateTokenDefinitionRequest(
    string Name,
    string ImageUrl,
    int Size,
    string Type
);
