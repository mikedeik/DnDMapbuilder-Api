namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Request to approve or reject a pending user registration.
/// </summary>
public record ApproveUserRequest(
    string UserId,
    bool Approved
);
