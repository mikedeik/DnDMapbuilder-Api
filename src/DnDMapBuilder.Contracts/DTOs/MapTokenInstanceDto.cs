namespace DnDMapBuilder.Contracts.DTOs;

/// <summary>
/// Data transfer object for a map token instance.
/// </summary>
public record MapTokenInstanceDto(
    string InstanceId,
    string TokenId,
    int X,
    int Y
);
