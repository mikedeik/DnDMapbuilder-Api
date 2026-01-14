using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> ApproveUserAsync(string userId, bool approved);
    Task<IEnumerable<UserDto>> GetPendingUsersAsync();
}

public interface ICampaignService
{
    Task<CampaignDto?> GetByIdAsync(string id, string userId);
    Task<IEnumerable<CampaignDto>> GetUserCampaignsAsync(string userId);
    Task<CampaignDto> CreateAsync(CreateCampaignRequest request, string userId);
    Task<CampaignDto?> UpdateAsync(string id, UpdateCampaignRequest request, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}

public interface IMissionService
{
    Task<MissionDto?> GetByIdAsync(string id, string userId);
    Task<IEnumerable<MissionDto>> GetByCampaignIdAsync(string campaignId, string userId);
    Task<MissionDto> CreateAsync(CreateMissionRequest request, string userId);
    Task<MissionDto?> UpdateAsync(string id, UpdateMissionRequest request, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}

public interface IGameMapService
{
    Task<GameMapDto?> GetByIdAsync(string id, string userId);
    Task<IEnumerable<GameMapDto>> GetByMissionIdAsync(string missionId, string userId);
    Task<GameMapDto> CreateAsync(CreateMapRequest request, string userId);
    Task<GameMapDto?> UpdateAsync(string id, UpdateMapRequest request, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}

public interface ITokenDefinitionService
{
    Task<TokenDefinitionDto?> GetByIdAsync(string id, string userId);
    Task<IEnumerable<TokenDefinitionDto>> GetUserTokensAsync(string userId);
    Task<TokenDefinitionDto> CreateAsync(CreateTokenDefinitionRequest request, string userId);
    Task<TokenDefinitionDto?> UpdateAsync(string id, UpdateTokenDefinitionRequest request, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}

public interface IJwtService
{
    string GenerateToken(string userId, string email, string role);
    string? ValidateToken(string token);
}
