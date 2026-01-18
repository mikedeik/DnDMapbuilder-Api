namespace DnDMapBuilder.Contracts.Responses;

/// <summary>
/// Response for paginated list results.
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
