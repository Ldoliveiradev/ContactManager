using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Models.Responses;

namespace ContactManager.Application.Contacts.Interfaces;

public interface IContactService
{
    Task<ContactListResponse> GetAllAsync(Guid userId, GetContactsRequest request, CancellationToken ct = default);
    Task<ContactResponse> GetByIdAsync(Guid userId, GetContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> CreateAsync(Guid userId, CreateContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> UpdateAsync(Guid userId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> DeleteAsync(Guid userId, DeleteContactRequest request, CancellationToken ct = default);
}
