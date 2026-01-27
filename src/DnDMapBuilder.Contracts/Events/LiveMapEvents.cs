using DnDMapBuilder.Contracts.DTOs;

namespace DnDMapBuilder.Contracts.Events;

/// <summary>
/// Event published when a game map is updated (name, grid properties, etc.)
/// </summary>
public record MapUpdatedEvent(
    string MapId,
    string Name,
    int Rows,
    int Cols,
    string GridColor,
    double GridOpacity,
    string? ImageUrl,
    DateTime Timestamp
);

/// <summary>
/// Event published when a token is moved on the map
/// </summary>
public record TokenMovedEvent(
    string MapId,
    string TokenInstanceId,
    int X,
    int Y,
    DateTime Timestamp
);

/// <summary>
/// Event published when a new token is added to the map
/// </summary>
public record TokenAddedEvent(
    string MapId,
    string TokenInstanceId,
    string TokenId,
    int X,
    int Y,
    DateTime Timestamp
);

/// <summary>
/// Event published when a token is removed from the map
/// </summary>
public record TokenRemovedEvent(
    string MapId,
    string TokenInstanceId,
    DateTime Timestamp
);

/// <summary>
/// Event published when a map's publication status changes
/// </summary>
public record MapStatusChangedEvent(
    string MapId,
    PublicationStatus NewStatus,
    DateTime Timestamp
);

/// <summary>
/// Complete snapshot of map state for new connections
/// </summary>
public record MapStateSnapshot(
    GameMapDto Map,
    DateTime Timestamp
);
