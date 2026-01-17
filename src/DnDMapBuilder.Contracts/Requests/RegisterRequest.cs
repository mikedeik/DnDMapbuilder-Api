namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request for user registration.
/// </summary>
public record RegisterRequest(
    string Username,
    string Email,
    string Password
);
