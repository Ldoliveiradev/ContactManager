using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Models.Dto;

namespace ContactManager.Application.Contacts.Models.Responses;

public sealed class ContactListResponse : BaseResponse<IReadOnlyList<ContactDto>>
{
    public static ContactListResponse Success(IReadOnlyList<ContactDto> items) =>
        new() { Data = items, IsSuccess = true };

    public static ContactListResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
