using ContactManager.Domain.ValueObjects;

namespace ContactManager.Domain.Models;

/// <summary>
/// The business identity of an account manager — the owner of a book of business
/// contacts. Authentication (username, password) is a separate concern handled by
/// the Application/Identity <c>User</c>; an account and its user share the same
/// <see cref="Id"/>, set together at registration.
/// </summary>
public sealed class AccountDomain
{
    public Guid Id { get; }
    public FullName FullName { get; private set; }
    public Email Email { get; private set; }

    private AccountDomain(Guid id, FullName fullName, Email email)
    {
        Id = id;
        FullName = fullName;
        Email = email;
    }

    public static AccountDomain Create(Guid id, string firstName, string lastName, string email)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Account id must not be empty.", nameof(id));

        return new AccountDomain(
            id,
            FullName.From(firstName, lastName),
            Email.From(email));
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FullName = FullName.From(firstName, lastName);
        Email = Email.From(email);
    }
}
