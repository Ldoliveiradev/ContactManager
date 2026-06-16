using ContactManager.Application.Common;

namespace ContactManager.Application.Contacts.Models.Requests;

public record GetContactsRequest(FilterRequest? Filter = null)
{
    public FilterRequest Filter { get; init; } = Filter ?? new FilterRequest();
}
