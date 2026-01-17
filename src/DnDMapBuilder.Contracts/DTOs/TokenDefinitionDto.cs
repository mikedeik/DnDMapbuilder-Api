namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a token definition.
/// </summary>
public record TokenDefinitionDto(
    string Id,
    string Name,
    string ImageUrl,
    int Size,
    string Type,
    string UserId,
    string? ImageFileId = null,
    string? ImageContentType = null,
    long ImageFileSize = 0
);
