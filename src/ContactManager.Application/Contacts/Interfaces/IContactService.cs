using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Models.Responses;

namespace ContactManager.Application.Contacts.Interfaces;

public interface IContactService
{
    Task<PaginationResponse<ContactListResponse>> GetAllAsync(Guid accountId, FilterRequest<GetContactRequest> filter, CancellationToken ct = default);
    Task<ContactResponse> GetByIdAsync(Guid accountId, GetContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> CreateAsync(Guid accountId, CreateContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> UpdateAsync(Guid accountId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> DeleteAsync(Guid accountId, DeleteContactRequest request, CancellationToken ct = default);
}
