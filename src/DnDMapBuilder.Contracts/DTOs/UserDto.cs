namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a user.
/// </summary>
public record UserDto(
    string Id,
    string Username,
    string Email,
    string Role,
    string Status,
    string? OAuthProvider = null,
    string? ProfilePictureUrl = null,
    bool EmailVerified = false
);
