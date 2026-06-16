using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Interfaces;
using ContactManager.Application.Contacts.Models.Dto;
using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Models.Responses;
using ContactManager.Application.Contacts.Validators;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Contacts.Services;

public sealed class ContactService(IContactRepository contacts) : IContactService
{
    private static readonly CreateContactRequestValidator CreateValidator = new();
    private static readonly UpdateContactRequestValidator UpdateValidator = new();

    public async Task<ContactListResponse> GetAllAsync(
        Guid userId, GetContactsRequest request, CancellationToken ct = default)
    {
        var f = request.Filter;
        var (items, totalCount) = await contacts.GetByUserAsync(
            userId, f.Search, f.SortBy, f.SortDesc, f.Page, f.PageSize, ct);

        return ContactListResponse.Success(
            items.Select(ToDto).ToList(),
            totalCount, f.Page, f.PageSize);
    }

    public async Task<ContactResponse> GetByIdAsync(
        Guid userId, GetContactRequest request, CancellationToken ct = default)
    {
        var contact = await contacts.GetByIdAsync(request.ContactId, ct);
        if (contact is null || contact.UserId != userId)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);

        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> CreateAsync(
        Guid userId, CreateContactRequest request, CancellationToken ct = default)
    {
        var validation = CreateValidator.Validate(request);
        if (!validation.IsValid)
            return ContactResponse.Failure(validation.Errors[0].ErrorMessage);

        var contact = ContactDomain.Create(Guid.NewGuid(), userId, request.Name, request.Email, request.Phone);
        await contacts.AddAsync(contact, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> UpdateAsync(
        Guid userId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default)
    {
        var validation = UpdateValidator.Validate(request);
        if (!validation.IsValid)
            return ContactResponse.Failure(validation.Errors[0].ErrorMessage);

        var contact = await contacts.GetByIdAsync(contactId, ct);
        if (contact is null || contact.UserId != userId)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);

        contact.Update(request.Name, request.Email, request.Phone);
        await contacts.UpdateAsync(contact, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> DeleteAsync(Guid userId, DeleteContactRequest request, CancellationToken ct = default)
    {
        var contact = await contacts.GetByIdAsync(request.ContactId, ct);
        if (contact is null || contact.UserId != userId)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);

        await contacts.DeleteAsync(contact.Id, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    private static ContactDto ToDto(ContactDomain c) => new(c.Id, c.Name, c.Email, c.Phone);
}
