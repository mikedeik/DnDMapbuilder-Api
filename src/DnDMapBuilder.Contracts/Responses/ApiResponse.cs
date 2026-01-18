namespace DnDMapBuilder.Contracts.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null
);
