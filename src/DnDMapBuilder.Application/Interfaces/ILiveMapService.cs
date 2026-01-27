using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Events;

namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for managing real-time game map broadcasts and live view state.
/// </summary>
public interface ILiveMapService
{
    /// <summary>
    /// Broadcasts a map update event to all connected clients subscribed to this map.
    /// Only broadcasts if the map's publication status is Live.
    /// </summary>
    /// <param name="mapId">The ID of the map that was updated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async broadcast operation</returns>
    Task BroadcastMapUpdateAsync(string mapId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts a token movement event to all connected clients subscribed to this map.
    /// Only broadcasts if the map's publication status is Live.
    /// </summary>
    /// <param name="mapId">The ID of the map containing the token</param>
    /// <param name="tokenInstanceId">The ID of the token instance that moved</param>
    /// <param name="x">The new X coordinate</param>
    /// <param name="y">The new Y coordinate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async broadcast operation</returns>
    Task BroadcastTokenMovedAsync(string mapId, string tokenInstanceId, int x, int y, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts a token addition event to all connected clients subscribed to this map.
    /// Only broadcasts if the map's publication status is Live.
    /// </summary>
    /// <param name="mapId">The ID of the map</param>
    /// <param name="tokenInstanceId">The ID of the newly added token instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async broadcast operation</returns>
    Task BroadcastTokenAddedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts a token removal event to all connected clients subscribed to this map.
    /// Only broadcasts if the map's publication status is Live.
    /// </summary>
    /// <param name="mapId">The ID of the map</param>
    /// <param name="tokenInstanceId">The ID of the token instance being removed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async broadcast operation</returns>
    Task BroadcastTokenRemovedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a map's publication status (Draft or Live) and broadcasts the change.
    /// </summary>
    /// <param name="mapId">The ID of the map</param>
    /// <param name="status">The new publication status</param>
    /// <param name="userId">The user ID making the change (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if user doesn't own the campaign containing this map</exception>
    Task SetMapPublicationStatusAsync(string mapId, DnDMapBuilder.Contracts.DTOs.PublicationStatus status, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a snapshot of the current map state for new connections.
    /// Returns null if the map is in Draft status or not found.
    /// </summary>
    /// <param name="mapId">The ID of the map</param>
    /// <param name="userId">The user ID making the request (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Map state snapshot for Live maps, null otherwise</returns>
    Task<MapStateSnapshot?> GetMapStateSnapshotAsync(string mapId, string userId, CancellationToken cancellationToken = default);
}
