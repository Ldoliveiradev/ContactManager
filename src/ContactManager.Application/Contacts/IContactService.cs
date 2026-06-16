namespace ContactManager.Application.Contacts;

/// <summary>
/// Application service for managing a user's contacts. All operations are scoped to the
/// calling user's id (supplied by the API from the JWT, never the request body).
/// </summary>
public interface IContactService
{
    Task<IReadOnlyList<ContactResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<ContactResponse> GetByIdAsync(Guid userId, Guid contactId, CancellationToken ct = default);
    Task<ContactResponse> CreateAsync(Guid userId, CreateContactRequest request, CancellationToken ct = default);
    Task<ContactResponse> UpdateAsync(Guid userId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid contactId, CancellationToken ct = default);
}
