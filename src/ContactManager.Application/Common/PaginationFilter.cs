namespace ContactManager.Application.Common;

public record PaginationFilter(
    string? Search = null,
    string? SortBy = null,
    bool SortDesc = false,
    int Page = 1,
    int PageSize = 10)
{
    public const int MaxPageSize = 200;

    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = NormalizePageSize(PageSize);

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1)
            return 6;

        return pageSize > MaxPageSize ? MaxPageSize : pageSize;
    }
}
