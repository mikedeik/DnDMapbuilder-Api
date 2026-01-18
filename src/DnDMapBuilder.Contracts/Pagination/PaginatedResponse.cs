namespace DnDMapBuilder.Contracts.Pagination;

/// <summary>
/// Generic paginated response wrapper for list operations.
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the PaginatedResponse class.
    /// </summary>
    /// <param name="items">The items in this page</param>
    /// <param name="pageNumber">The current page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="totalCount">The total number of items</param>
    public PaginatedResponse(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items.ToList();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Gets the items in this page.
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Gets a value indicating whether there are more pages after the current page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there are pages before the current page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
