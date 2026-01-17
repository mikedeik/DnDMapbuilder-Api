namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a game map.
/// </summary>
public record GameMapDto(
    string Id,
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    List<MapTokenInstanceDto> Tokens,
    string GridColor,
    double GridOpacity,
    string MissionId,
    string? ImageFileId = null,
    string? ImageContentType = null,
    long ImageFileSize = 0
);
