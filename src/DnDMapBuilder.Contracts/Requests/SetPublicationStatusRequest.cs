using DnDMapBuilder.Contracts.DTOs;

namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to set the publication status of a game map.
/// </summary>
public record SetPublicationStatusRequest(PublicationStatus Status);
