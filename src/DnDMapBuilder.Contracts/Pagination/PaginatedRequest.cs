namespace DnDMapBuilder.Contracts.Pagination;

/// <summary>
/// Base class for paginated requests.
/// </summary>
public class PaginatedRequest
{
    /// <summary>
    /// Gets or sets the page number (1-based, default is 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size (default is 20, max is 100).
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Validates the pagination parameters.
    /// </summary>
    public void Validate()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 20;
        else if (PageSize > 100)
            PageSize = 100;
    }

    /// <summary>
    /// Gets the number of items to skip for the current page.
    /// </summary>
    public int GetSkipCount() => (PageNumber - 1) * PageSize;
}
