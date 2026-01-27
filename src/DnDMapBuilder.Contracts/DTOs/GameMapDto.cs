namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Publication status for game maps used in live view feature.
/// </summary>
public enum PublicationStatus
{
    /// <summary>Draft maps are editable but not broadcast to live views.</summary>
    Draft = 0,

    /// <summary>Live maps are broadcast in real-time to connected clients.</summary>
    Live = 1
}

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
    PublicationStatus PublicationStatus = PublicationStatus.Draft,
    string? ImageFileId = null,
    string? ImageContentType = null,
    long ImageFileSize = 0
);
