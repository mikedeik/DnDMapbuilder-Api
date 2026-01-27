using DnDMapBuilder.Contracts.DTOs;

namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to create a new game map.
/// </summary>
public record CreateMapRequest(
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    string GridColor,
    double GridOpacity,
    string MissionId,
    PublicationStatus PublicationStatus = PublicationStatus.Draft
);
