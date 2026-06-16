namespace ContactManager.Application.Common;

public record FilterRequest(
    string? Search = null,
    string? SortBy = null,
    bool SortDesc = false,
    int Page = 1,
    int PageSize = 10);
