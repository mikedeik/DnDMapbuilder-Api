using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetPendingUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
}

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IEnumerable<Campaign>> GetByOwnerIdAsync(string ownerId);
    Task<Campaign?> GetWithMissionsAsync(string id);
    Task<Campaign?> GetCompleteAsync(string id); // With all related data
}

public interface IMissionRepository : IRepository<Mission>
{
    Task<IEnumerable<Mission>> GetByCampaignIdAsync(string campaignId);
    Task<Mission?> GetWithMapsAsync(string id);
}

public interface IGameMapRepository : IRepository<GameMap>
{
    Task<IEnumerable<GameMap>> GetByMissionIdAsync(string missionId);
    Task<GameMap?> GetWithTokensAsync(string id);
}

public interface ITokenDefinitionRepository : IRepository<TokenDefinition>
{
    Task<IEnumerable<TokenDefinition>> GetByUserIdAsync(string userId);
    Task<IEnumerable<TokenDefinition>> GetByTypeAsync(string type);
}

public interface IMapTokenInstanceRepository : IRepository<MapTokenInstance>
{
    Task<IEnumerable<MapTokenInstance>> GetByMapIdAsync(string mapId);
    Task DeleteByMapIdAsync(string mapId);
}
