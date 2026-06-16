namespace ContactManager.Application.Common;

public sealed class PaginationResponse<T> : BaseResponse<T>
{
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginationResponse<T> Success(T data, int totalCount, int page, int pageSize) =>
        new() { Data = data, IsSuccess = true, TotalCount = totalCount, Page = page, PageSize = pageSize };

    public static PaginationResponse<T> Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
