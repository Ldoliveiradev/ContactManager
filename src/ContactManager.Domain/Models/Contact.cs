using ContactManager.Domain.Events;
using ContactManager.Domain.Primitives;
using ContactManager.Domain.ValueObjects;

namespace ContactManager.Domain.Models;

public sealed class Contact : AggregateRoot
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public ContactName Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }

    private Contact(Guid id, Guid accountId, ContactName name, Email email, PhoneNumber? phone)
    {
        Id = id;
        AccountId = accountId;
        Name = name;
        Email = email;
        Phone = phone;
    }

    public static Contact Create(Guid id, Guid accountId, string name, string email, string? phone)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Contact id must not be empty.", nameof(id));
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account id must not be empty.", nameof(accountId));

        var contact = new Contact(
            id,
            accountId,
            ContactName.From(name),
            Email.From(email),
            string.IsNullOrWhiteSpace(phone) ? null : PhoneNumber.From(phone));

        contact.RaiseDomainEvent(new ContactCreatedEvent(
            Guid.NewGuid(), DateTime.UtcNow, id, accountId, name.Trim(), email.Trim().ToLowerInvariant()));

        return contact;
    }

    public void Update(string name, string email, string? phone)
    {
        Name = ContactName.From(name);
        Email = Email.From(email);
        Phone = string.IsNullOrWhiteSpace(phone) ? null : PhoneNumber.From(phone);

        RaiseDomainEvent(new ContactUpdatedEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId, Name.Value, Email.Value));
    }

    public void Delete()
    {
        RaiseDomainEvent(new ContactDeletedEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId));
    }
}
