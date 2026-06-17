using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Models.Dto;

namespace ContactManager.Application.Contacts.Models.Responses;

public sealed class ContactResponse : BaseResponse<ContactDto>
{
    public static ContactResponse Success(ContactDto data) =>
        new() { Data = data, IsSuccess = true };

    public static ContactResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
