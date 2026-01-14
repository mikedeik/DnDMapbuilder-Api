using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Mappings;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Data.Entities;
using DnDMapBuilder.Data.Repositories;

namespace DnDMapBuilder.Application.Services;

public class GameMapService : IGameMapService
{
    private readonly IGameMapRepository _mapRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IMapTokenInstanceRepository _tokenInstanceRepository;

    public GameMapService(
        IGameMapRepository mapRepository,
        IMissionRepository missionRepository,
        ICampaignRepository campaignRepository,
        IMapTokenInstanceRepository tokenInstanceRepository)
    {
        _mapRepository = mapRepository;
        _missionRepository = missionRepository;
        _campaignRepository = campaignRepository;
        _tokenInstanceRepository = tokenInstanceRepository;
    }

    public async Task<GameMapDto?> GetByIdAsync(string id, string userId)
    {
        var map = await _mapRepository.GetWithTokensAsync(id);
        if (map == null)
        {
            return null;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId))
        {
            return null;
        }

        return map.ToDto();
    }

    public async Task<IEnumerable<GameMapDto>> GetByMissionIdAsync(string missionId, string userId)
    {
        if (!await HasAccessToMapAsync(missionId, userId))
        {
            return Enumerable.Empty<GameMapDto>();
        }

        var maps = await _mapRepository.GetByMissionIdAsync(missionId);
        return maps.Select(m => m.ToDto());
    }

    public async Task<GameMapDto> CreateAsync(CreateMapRequest request, string userId)
    {
        if (!await HasAccessToMapAsync(request.MissionId, userId))
        {
            throw new UnauthorizedAccessException("You don't have permission to add maps to this mission.");
        }

        var map = new GameMap
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Rows = request.Rows,
            Cols = request.Cols,
            GridColor = request.GridColor,
            GridOpacity = request.GridOpacity,
            MissionId = request.MissionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mapRepository.AddAsync(map);
        return map.ToDto();
    }

    public async Task<GameMapDto?> UpdateAsync(string id, UpdateMapRequest request, string userId)
    {
        var map = await _mapRepository.GetByIdAsync(id);
        if (map == null)
        {
            return null;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId))
        {
            return null;
        }

        map.Name = request.Name;
        map.ImageUrl = request.ImageUrl;
        map.Rows = request.Rows;
        map.Cols = request.Cols;
        map.GridColor = request.GridColor;
        map.GridOpacity = request.GridOpacity;
        map.UpdatedAt = DateTime.UtcNow;

        // Update tokens
        await _tokenInstanceRepository.DeleteByMapIdAsync(map.Id);
        
        foreach (var tokenReq in request.Tokens)
        {
            var tokenInstance = new MapTokenInstance
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = tokenReq.TokenId,
                MapId = map.Id,
                X = tokenReq.X,
                Y = tokenReq.Y,
                CreatedAt = DateTime.UtcNow
            };
            await _tokenInstanceRepository.AddAsync(tokenInstance);
        }

        await _mapRepository.UpdateAsync(map);
        
        var updatedMap = await _mapRepository.GetWithTokensAsync(id);
        return updatedMap?.ToDto();
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var map = await _mapRepository.GetByIdAsync(id);
        if (map == null)
        {
            return false;
        }

        if (!await HasAccessToMapAsync(map.MissionId, userId))
        {
            return false;
        }

        await _mapRepository.DeleteAsync(id);
        return true;
    }

    private async Task<bool> HasAccessToMapAsync(string missionId, string userId)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId);
        if (mission == null)
        {
            return false;
        }

        var campaign = await _campaignRepository.GetByIdAsync(mission.CampaignId);
        return campaign != null && campaign.OwnerId == userId;
    }
}

public class TokenDefinitionService : ITokenDefinitionService
{
    private readonly ITokenDefinitionRepository _tokenRepository;

    public TokenDefinitionService(ITokenDefinitionRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<TokenDefinitionDto?> GetByIdAsync(string id, string userId)
    {
        var token = await _tokenRepository.GetByIdAsync(id);
        if (token == null || token.UserId != userId)
        {
            return null;
        }

        return token.ToDto();
    }

    public async Task<IEnumerable<TokenDefinitionDto>> GetUserTokensAsync(string userId)
    {
        var tokens = await _tokenRepository.GetByUserIdAsync(userId);
        return tokens.Select(t => t.ToDto());
    }

    public async Task<TokenDefinitionDto> CreateAsync(CreateTokenDefinitionRequest request, string userId)
    {
        var token = new TokenDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Size = request.Size,
            Type = request.Type,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(token);
        return token.ToDto();
    }

    public async Task<TokenDefinitionDto?> UpdateAsync(string id, UpdateTokenDefinitionRequest request, string userId)
    {
        var token = await _tokenRepository.GetByIdAsync(id);
        if (token == null || token.UserId != userId)
        {
            return null;
        }

        token.Name = request.Name;
        token.ImageUrl = request.ImageUrl;
        token.Size = request.Size;
        token.Type = request.Type;
        token.UpdatedAt = DateTime.UtcNow;

        await _tokenRepository.UpdateAsync(token);
        return token.ToDto();
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var token = await _tokenRepository.GetByIdAsync(id);
        if (token == null || token.UserId != userId)
        {
            return false;
        }

        await _tokenRepository.DeleteAsync(id);
        return true;
    }
}
