using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories;
using DnDMapBuilder.Data.Repositories.Interfaces;

namespace DnDMapBuilder.Application.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly ICampaignRepository _campaignRepository;

    public MissionService(IMissionRepository missionRepository, ICampaignRepository campaignRepository)
    {
        _missionRepository = missionRepository;
        _campaignRepository = campaignRepository;
    }

    public async Task<MissionDto?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetWithMapsAsync(id, cancellationToken);
        if (mission == null)
        {
            return null;
        }

        var campaign = await _campaignRepository.GetByIdAsync(mission.CampaignId, cancellationToken);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return null;
        }

        return mission.ToDto();
    }

    public async Task<IEnumerable<MissionDto>> GetByCampaignIdAsync(string campaignId, string userId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return Enumerable.Empty<MissionDto>();
        }

        var missions = await _missionRepository.GetByCampaignIdAsync(campaignId, cancellationToken);
        return missions.Select(m => m.ToDto());
    }

    public async Task<MissionDto> CreateAsync(CreateMissionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null || campaign.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to add missions to this campaign.");
        }

        var mission = new Mission
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            CampaignId = request.CampaignId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _missionRepository.AddAsync(mission, cancellationToken);
        return mission.ToDto();
    }

    public async Task<MissionDto?> UpdateAsync(string id, UpdateMissionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);
        if (mission == null)
        {
            return null;
        }

        var campaign = await _campaignRepository.GetByIdAsync(mission.CampaignId, cancellationToken);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return null;
        }

        mission.Name = request.Name;
        mission.Description = request.Description;
        mission.UpdatedAt = DateTime.UtcNow;

        await _missionRepository.UpdateAsync(mission, cancellationToken);
        return mission.ToDto();
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var mission = await _missionRepository.GetByIdAsync(id, cancellationToken);
        if (mission == null)
        {
            return false;
        }

        var campaign = await _campaignRepository.GetByIdAsync(mission.CampaignId, cancellationToken);
        if (campaign == null || campaign.OwnerId != userId)
        {
            return false;
        }

        await _missionRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
