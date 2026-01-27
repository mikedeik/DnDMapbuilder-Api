using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Data.Entities;
using PublicationStatus = DnDMapBuilder.Contracts.DTOs.PublicationStatus;

namespace DnDMapBuilder.Application.Mappings;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.Status,
            user.OAuthProvider,
            user.ProfilePictureUrl,
            user.EmailVerified
        );
    }

    public static TokenDefinitionDto ToDto(this TokenDefinition token)
    {
        return new TokenDefinitionDto(
            token.Id,
            token.Name,
            token.ImageUrl,
            token.Size,
            token.Type,
            token.UserId,
            token.ImageFileId,
            token.ImageContentType,
            token.ImageFileSize
        );
    }

    public static MapTokenInstanceDto ToDto(this MapTokenInstance instance)
    {
        return new MapTokenInstanceDto(
            instance.Id,
            instance.TokenId,
            instance.X,
            instance.Y
        );
    }

    public static GameMapDto ToDto(this GameMap map)
    {
        return new GameMapDto(
            map.Id,
            map.Name,
            map.ImageUrl,
            map.Rows,
            map.Cols,
            map.Tokens.Select(t => t.ToDto()).ToList(),
            map.GridColor,
            map.GridOpacity,
            map.MissionId,
            (PublicationStatus)(int)map.PublicationStatus,
            map.ImageFileId,
            map.ImageContentType,
            map.ImageFileSize
        );
    }

    public static MissionDto ToDto(this Mission mission)
    {
        return new MissionDto(
            mission.Id,
            mission.Name,
            mission.Description,
            mission.Maps.Select(m => m.ToDto()).ToList(),
            mission.CampaignId
        );
    }

    public static CampaignDto ToDto(this Campaign campaign)
    {
        return new CampaignDto(
            campaign.Id,
            campaign.Name,
            campaign.Description,
            campaign.Missions.Select(m => m.ToDto()).ToList(),
            campaign.OwnerId,
            campaign.CreatedAt,
            campaign.UpdatedAt
        );
    }
}
