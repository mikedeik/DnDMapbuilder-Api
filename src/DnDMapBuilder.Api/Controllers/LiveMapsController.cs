using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Events;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

/// <summary>
/// Controller for managing live map publication and viewing.
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class LiveMapsController : ControllerBase
{
    private readonly ILiveMapService _liveMapService;

    public LiveMapsController(ILiveMapService liveMapService)
    {
        _liveMapService = liveMapService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Sets the publication status of a game map (Draft or Live).
    /// Live maps are broadcast in real-time to connected clients.
    /// </summary>
    /// <param name="mapId">The ID of the map to update</param>
    /// <param name="request">Request containing the new publication status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response with the new status</returns>
    [HttpPut("{mapId}/status")]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult<ApiResponse<bool>>> SetPublicationStatus(
        string mapId,
        [FromBody] SetPublicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _liveMapService.SetMapPublicationStatusAsync(mapId, request.Status, userId, cancellationToken);
            return Ok(new ApiResponse<bool>(true, true, "Publication status updated."));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Map not found."));
        }
    }

    /// <summary>
    /// Gets a snapshot of the current state of a live map.
    /// Only returns data for maps with Live publication status.
    /// New clients use this to get initial map state when connecting.
    /// </summary>
    /// <param name="mapId">The ID of the map to get snapshot for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Map state snapshot or 404 if map is not live</returns>
    [HttpGet("{mapId}/snapshot")]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult<ApiResponse<MapStateSnapshot>>> GetSnapshot(
        string mapId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var snapshot = await _liveMapService.GetMapStateSnapshotAsync(mapId, userId, cancellationToken);

        if (snapshot == null)
        {
            return NotFound(new ApiResponse<MapStateSnapshot>(false, null, "Map not found or not live."));
        }

        return Ok(new ApiResponse<MapStateSnapshot>(true, snapshot));
    }
}
