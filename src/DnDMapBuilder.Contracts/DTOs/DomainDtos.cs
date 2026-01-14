namespace DnDMapBuilder.Contracts.DTOs;

public record UserDto(
    string Id,
    string Username,
    string Email,
    string Role,
    string Status
);

public record TokenDefinitionDto(
    string Id,
    string Name,
    string ImageUrl,
    int Size,
    string Type,
    string UserId
);

public record MapTokenInstanceDto(
    string InstanceId,
    string TokenId,
    int X,
    int Y
);

public record GameMapDto(
    string Id,
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    List<MapTokenInstanceDto> Tokens,
    string GridColor,
    double GridOpacity,
    string MissionId
);

public record MissionDto(
    string Id,
    string Name,
    string Description,
    List<GameMapDto> Maps,
    string CampaignId
);

public record CampaignDto(
    string Id,
    string Name,
    string Description,
    List<MissionDto> Missions,
    string OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
