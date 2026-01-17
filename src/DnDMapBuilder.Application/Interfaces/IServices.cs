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

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage and returns the file ID for database reference.
    /// </summary>
    /// <param name="file">The file stream to upload</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="storageCategory">Category for organization (e.g., "maps", "tokens")</param>
    /// <returns>Generated file ID</returns>
    Task<string> UploadAsync(Stream file, string fileName, string contentType, string storageCategory);

    /// <summary>
    /// Gets the public URL for a stored file.
    /// </summary>
    /// <param name="fileId">The file ID returned from upload</param>
    /// <param name="storageCategory">The category where the file is stored</param>
    /// <returns>Public URL to the file</returns>
    string GetPublicUrl(string fileId, string storageCategory);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileId">The file ID to delete</param>
    /// <param name="storageCategory">The category where the file is stored</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string fileId, string storageCategory);
}
