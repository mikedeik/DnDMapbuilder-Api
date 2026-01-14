namespace DnDMapBuilder.Contracts.Responses;

public record AuthResponse(
    string Token,
    string UserId,
    string Username,
    string Email,
    string Role,
    string Status
);

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null
);

public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
