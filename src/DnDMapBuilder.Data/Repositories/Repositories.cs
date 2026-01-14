using Microsoft.EntityFrameworkCore;
using DnDMapBuilder.Data.Entities;

namespace DnDMapBuilder.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DnDMapBuilderDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DnDMapBuilderDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetPendingUsersAsync()
    {
        return await _dbSet.Where(u => u.Status == "pending").ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }
}

public class CampaignRepository : Repository<Campaign>, ICampaignRepository
{
    public CampaignRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<Campaign>> GetByOwnerIdAsync(string ownerId)
    {
        return await _dbSet
            .Where(c => c.OwnerId == ownerId)
            .Include(c => c.Missions)
            .ToListAsync();
    }

    public async Task<Campaign?> GetWithMissionsAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Missions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Campaign?> GetCompleteAsync(string id)
    {
        return await _dbSet
            .Include(c => c.Missions)
                .ThenInclude(m => m.Maps)
                    .ThenInclude(map => map.Tokens)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

public class MissionRepository : Repository<Mission>, IMissionRepository
{
    public MissionRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<Mission>> GetByCampaignIdAsync(string campaignId)
    {
        return await _dbSet
            .Where(m => m.CampaignId == campaignId)
            .Include(m => m.Maps)
            .ToListAsync();
    }

    public async Task<Mission?> GetWithMapsAsync(string id)
    {
        return await _dbSet
            .Include(m => m.Maps)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

public class GameMapRepository : Repository<GameMap>, IGameMapRepository
{
    public GameMapRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<GameMap>> GetByMissionIdAsync(string missionId)
    {
        return await _dbSet
            .Where(m => m.MissionId == missionId)
            .Include(m => m.Tokens)
            .ToListAsync();
    }

    public async Task<GameMap?> GetWithTokensAsync(string id)
    {
        return await _dbSet
            .Include(m => m.Tokens)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

public class TokenDefinitionRepository : Repository<TokenDefinition>, ITokenDefinitionRepository
{
    public TokenDefinitionRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<TokenDefinition>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TokenDefinition>> GetByTypeAsync(string type)
    {
        return await _dbSet
            .Where(t => t.Type == type)
            .ToListAsync();
    }
}

public class MapTokenInstanceRepository : Repository<MapTokenInstance>, IMapTokenInstanceRepository
{
    public MapTokenInstanceRepository(DnDMapBuilderDbContext context) : base(context) { }

    public async Task<IEnumerable<MapTokenInstance>> GetByMapIdAsync(string mapId)
    {
        return await _dbSet
            .Where(t => t.MapId == mapId)
            .Include(t => t.Token)
            .ToListAsync();
    }

    public async Task DeleteByMapIdAsync(string mapId)
    {
        var tokens = await _dbSet.Where(t => t.MapId == mapId).ToListAsync();
        _dbSet.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
}
