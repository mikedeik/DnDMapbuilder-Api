namespace DnDMapBuilder.Contracts.Responses;

/// <summary>
/// Response containing authentication data.
/// </summary>
public record AuthResponse(
    string Token,
    string UserId,
    string Username,
    string Email,
    string Role,
    string Status
);
