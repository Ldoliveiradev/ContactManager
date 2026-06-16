using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Models.Dto;

namespace ContactManager.Application.Contacts.Models.Responses;

public sealed class ContactListResponse
{
    public IReadOnlyList<ContactDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static ContactListResponse Success(IReadOnlyList<ContactDto> items, int totalCount, int page, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize, IsSuccess = true };

    public static ContactListResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
