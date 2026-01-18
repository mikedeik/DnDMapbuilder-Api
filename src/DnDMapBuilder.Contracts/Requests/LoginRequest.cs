namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request for user login.
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);
