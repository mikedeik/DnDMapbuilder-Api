using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

/// <summary>
/// Controller for managing missions within campaigns.
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Gets a mission by ID.
    /// </summary>
    /// <param name="id">The mission ID</param>
    /// <returns>The mission details or 404 if not found</returns>
    [HttpGet("{id}")]
    [ResponseCache(CacheProfileName = "Long300")]
    public async Task<ActionResult<ApiResponse<MissionDto>>> GetMission(string id)
    {
        var mission = await _missionService.GetByIdAsync(id, GetUserId());

        if (mission == null)
        {
            return NotFound(new ApiResponse<MissionDto>(false, null, "Mission not found."));
        }

        return Ok(new ApiResponse<MissionDto>(true, mission));
    }

    /// <summary>
    /// Gets all missions for a specific campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID</param>
    /// <returns>Collection of missions in the campaign</returns>
    [HttpGet("campaign/{campaignId}")]
    [ResponseCache(CacheProfileName = "Long300")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MissionDto>>>> GetMissionsByCampaign(string campaignId)
    {
        var missions = await _missionService.GetByCampaignIdAsync(campaignId, GetUserId());
        return Ok(new ApiResponse<IEnumerable<MissionDto>>(true, missions));
    }

    /// <summary>
    /// Creates a new mission in a campaign.
    /// </summary>
    /// <param name="request">Create mission request</param>
    /// <returns>The created mission details or 403 if unauthorized</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MissionDto>>> CreateMission([FromBody] CreateMissionRequest request)
    {
        try
        {
            var mission = await _missionService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetMission), new { id = mission.Id }, new ApiResponse<MissionDto>(true, mission, "Mission created."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing mission.
    /// </summary>
    /// <param name="id">The mission ID to update</param>
    /// <param name="request">Update mission request</param>
    /// <returns>The updated mission details or 404 if not found</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MissionDto>>> UpdateMission(string id, [FromBody] UpdateMissionRequest request)
    {
        var mission = await _missionService.UpdateAsync(id, request, GetUserId());

        if (mission == null)
        {
            return NotFound(new ApiResponse<MissionDto>(false, null, "Mission not found."));
        }

        return Ok(new ApiResponse<MissionDto>(true, mission, "Mission updated."));
    }

    /// <summary>
    /// Deletes a mission.
    /// </summary>
    /// <param name="id">The mission ID to delete</param>
    /// <returns>Success indicator or 404 if not found</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMission(string id)
    {
        var result = await _missionService.DeleteAsync(id, GetUserId());

        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Mission not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "Mission deleted."));
    }
}
