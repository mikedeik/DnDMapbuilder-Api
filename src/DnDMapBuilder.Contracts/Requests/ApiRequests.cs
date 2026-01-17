namespace DnDMapBuilder.Contracts.Requests;

public record RegisterRequest(
    string Username,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record CreateCampaignRequest(
    string Name,
    string Description
);

public record UpdateCampaignRequest(
    string Name,
    string Description
);

public record CreateMissionRequest(
    string Name,
    string Description,
    string CampaignId
);

public record UpdateMissionRequest(
    string Name,
    string Description
);

public record CreateMapRequest(
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    string GridColor,
    double GridOpacity,
    string MissionId
);

public record UpdateMapRequest(
    string Name,
    string? ImageUrl,
    int Rows,
    int Cols,
    List<MapTokenInstanceRequest> Tokens,
    string GridColor,
    double GridOpacity
);

public record MapTokenInstanceRequest(
    string TokenId,
    int X,
    int Y
);

public record CreateTokenDefinitionRequest(
    string Name,
    string ImageUrl,
    int Size,
    string Type
);

public record UpdateTokenDefinitionRequest(
    string Name,
    string ImageUrl,
    int Size,
    string Type
);

public record ApproveUserRequest(
    string UserId,
    bool Approved
);

// File upload response
public record ImageUploadResponse(
    string FileId,
    string Url,
    string ContentType,
    long FileSize
);
