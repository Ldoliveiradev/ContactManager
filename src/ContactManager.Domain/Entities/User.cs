namespace ContactManager.Domain.Entities;

/// <summary>
/// A user who can authenticate and own contacts. Created only through <see cref="Create"/>
/// so its invariants (non-empty id, username, and password hash) are always enforced.
/// </summary>
public sealed class User
{
    public Guid Id { get; }
    public string Username { get; }
    public string PasswordHash { get; }

    private User(Guid id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }

    public static User Create(Guid id, string username, string passwordHash)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User(id, username.Trim(), passwordHash);
    }
}
