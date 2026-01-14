using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CampaignDto>>>> GetUserCampaigns()
    {
        var campaigns = await _campaignService.GetUserCampaignsAsync(GetUserId());
        return Ok(new ApiResponse<IEnumerable<CampaignDto>>(true, campaigns));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> GetCampaign(string id)
    {
        var campaign = await _campaignService.GetByIdAsync(id, GetUserId());
        
        if (campaign == null)
        {
            return NotFound(new ApiResponse<CampaignDto>(false, null, "Campaign not found."));
        }

        return Ok(new ApiResponse<CampaignDto>(true, campaign));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var campaign = await _campaignService.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, new ApiResponse<CampaignDto>(true, campaign, "Campaign created."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CampaignDto>>> UpdateCampaign(string id, [FromBody] UpdateCampaignRequest request)
    {
        var campaign = await _campaignService.UpdateAsync(id, request, GetUserId());
        
        if (campaign == null)
        {
            return NotFound(new ApiResponse<CampaignDto>(false, null, "Campaign not found."));
        }

        return Ok(new ApiResponse<CampaignDto>(true, campaign, "Campaign updated."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCampaign(string id)
    {
        var result = await _campaignService.DeleteAsync(id, GetUserId());
        
        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Campaign not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "Campaign deleted."));
    }
}
