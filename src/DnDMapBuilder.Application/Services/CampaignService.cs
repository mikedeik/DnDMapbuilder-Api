using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories;

namespace DnDMapBuilder.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;

    public CampaignService(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignDto?> GetByIdAsync(string id, string userId)
    {
        var campaign = await _campaignRepository.GetCompleteAsync(id);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return null;
        }

        return campaign.ToDto();
    }

    public async Task<IEnumerable<CampaignDto>> GetUserCampaignsAsync(string userId)
    {
        var campaigns = await _campaignRepository.GetByOwnerIdAsync(userId);
        return campaigns.Select(c => c.ToDto());
    }

    public async Task<CampaignDto> CreateAsync(CreateCampaignRequest request, string userId)
    {
        var campaign = new Campaign
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _campaignRepository.AddAsync(campaign);
        return campaign.ToDto();
    }

    public async Task<CampaignDto?> UpdateAsync(string id, UpdateCampaignRequest request, string userId)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return null;
        }

        campaign.Name = request.Name;
        campaign.Description = request.Description;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign);
        return campaign.ToDto();
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return false;
        }

        await _campaignRepository.DeleteAsync(id);
        return true;
    }
}
