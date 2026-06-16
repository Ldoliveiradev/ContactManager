using ContactManager.Domain.Primitives;
using ContactManager.Domain.ValueObjects;

namespace ContactManager.Domain.Models;

public sealed class AccountDomain : AggregateRoot
{
    public Guid Id { get; }
    public Username Username { get; private set; }
    public FullName FullName { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    private AccountDomain(Guid id, Username username, FullName fullName, Email email, string passwordHash)
    {
        Id = id;
        Username = username;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
    }

    public static AccountDomain Create(
        Guid id,
        string username,
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Account id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new AccountDomain(
            id,
            Username.From(username),
            FullName.From(firstName, lastName),
            Email.From(email),
            passwordHash);
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FullName = FullName.From(firstName, lastName);
        Email = Email.From(email);
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }
}
