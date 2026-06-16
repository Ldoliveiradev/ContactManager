using ContactManager.Application.Abstractions;
using ContactManager.Application.Exceptions;
using ContactManager.Domain.Entities;

namespace ContactManager.Application.Contacts;

/// <summary>
/// Business logic for contacts. Every operation is scoped to the calling user's id
/// (supplied by the API from the JWT, never from the request body). Contacts owned by
/// another user are treated as not-found so ownership is never leaked (IDOR protection).
/// </summary>
public sealed class ContactService(IContactRepository contacts) : IContactService
{
    public async Task<IReadOnlyList<ContactResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await contacts.GetByUserAsync(userId, ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<ContactResponse> GetByIdAsync(Guid userId, Guid contactId, CancellationToken ct = default)
    {
        var contact = await GetOwnedOrThrow(userId, contactId, ct);
        return ToResponse(contact);
    }

    public async Task<ContactResponse> CreateAsync(Guid userId, CreateContactRequest request, CancellationToken ct = default)
    {
        Contact contact;
        try
        {
            contact = Contact.Create(Guid.NewGuid(), userId, request.Name, request.Email, request.Phone);
        }
        catch (ArgumentException ex)
        {
            // Translate domain invariant violations into an Application validation error.
            throw new ValidationException(ex.Message);
        }

        await contacts.AddAsync(contact, ct);
        return ToResponse(contact);
    }

    public async Task<ContactResponse> UpdateAsync(Guid userId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default)
    {
        var contact = await GetOwnedOrThrow(userId, contactId, ct);

        try
        {
            contact.Update(request.Name, request.Email, request.Phone);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(ex.Message);
        }

        await contacts.UpdateAsync(contact, ct);
        return ToResponse(contact);
    }

    public async Task DeleteAsync(Guid userId, Guid contactId, CancellationToken ct = default)
    {
        var contact = await GetOwnedOrThrow(userId, contactId, ct);
        await contacts.DeleteAsync(contact.Id, ct);
    }

    private async Task<Contact> GetOwnedOrThrow(Guid userId, Guid contactId, CancellationToken ct)
    {
        var contact = await contacts.GetByIdAsync(contactId, ct);
        if (contact is null || contact.UserId != userId)
            throw new NotFoundException("Contact not found.");
        return contact;
    }

    private static ContactResponse ToResponse(Contact c) => new(c.Id, c.Name, c.Email, c.Phone);
}
