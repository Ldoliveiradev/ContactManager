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

    public async Task<PaginationResponse<IReadOnlyList<ContactDto>>> GetAllAsync(
        Guid accountId, PaginationFilter filter, CancellationToken ct = default)
    {
        var (items, totalCount) = await contacts.GetByAccountAsync(
            accountId, filter.Search, filter.SortBy, filter.SortDesc, filter.Page, filter.PageSize, ct);

        return PaginationResponse<IReadOnlyList<ContactDto>>.Success(
            items.Select(ToDto).ToList(), totalCount, filter.Page, filter.PageSize);
    }

    public async Task<ContactResponse> GetByIdAsync(
        Guid accountId, GetContactRequest request, CancellationToken ct = default)
    {
        var contact = await contacts.GetByIdAsync(request.ContactId, ct);
        if (contact is null)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);
        if (contact.AccountId != accountId)
            return ContactResponse.Failure(ErrorMessages.Auth.Forbidden);

        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> CreateAsync(
        Guid accountId, CreateContactRequest request, CancellationToken ct = default)
    {
        var validation = CreateValidator.Validate(request);
        if (!validation.IsValid)
            return ContactResponse.Failure(validation.Errors[0].ErrorMessage);

        if (await contacts.ExistsByEmailAsync(accountId, request.Email, excludeId: null, ct))
            return ContactResponse.Failure(ErrorMessages.Contact.EmailDuplicate);

        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, request.Name, request.Email, request.Phone);
        await contacts.AddAsync(contact, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> UpdateAsync(
        Guid accountId, Guid contactId, UpdateContactRequest request, CancellationToken ct = default)
    {
        var validation = UpdateValidator.Validate(request);
        if (!validation.IsValid)
            return ContactResponse.Failure(validation.Errors[0].ErrorMessage);

        var contact = await contacts.GetByIdAsync(contactId, ct);
        if (contact is null)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);
        if (contact.AccountId != accountId)
            return ContactResponse.Failure(ErrorMessages.Auth.Forbidden);

        if (await contacts.ExistsByEmailAsync(accountId, request.Email, excludeId: contactId, ct))
            return ContactResponse.Failure(ErrorMessages.Contact.EmailDuplicate);

        contact.Update(request.Name, request.Email, request.Phone);
        await contacts.UpdateAsync(contact, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    public async Task<ContactResponse> DeleteAsync(
        Guid accountId, DeleteContactRequest request, CancellationToken ct = default)
    {
        var contact = await contacts.GetByIdAsync(request.ContactId, ct);
        if (contact is null)
            return ContactResponse.Failure(ErrorMessages.Contact.NotFound);
        if (contact.AccountId != accountId)
            return ContactResponse.Failure(ErrorMessages.Auth.Forbidden);

        await contacts.DeleteAsync(contact.Id, ct);
        return ContactResponse.Success(ToDto(contact));
    }

    private static ContactDto ToDto(ContactDomain c) =>
        new(c.Id, c.Name.Value, c.Email.Value, c.Phone?.Value);
}
