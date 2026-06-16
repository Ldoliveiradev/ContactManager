using ContactManager.Application.Contacts.Models.Dto;

namespace ContactManager.Application.Contacts.Models.Responses;

public sealed class ContactResponse
{
    public ContactDto? Data { get; init; }

    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static ContactResponse Success(ContactDto data) =>
        new() { Data = data, IsSuccess = true };

    public static ContactResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
