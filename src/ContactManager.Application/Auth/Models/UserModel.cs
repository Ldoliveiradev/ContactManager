namespace ContactManager.Application.Auth.Models;

/// <summary>
/// The authentication identity (login). This is an application/auth concern, not
/// a business-domain entity — it holds only the credentials needed to log in.
/// A user and their business <c>AccountDomain</c> share the same <see cref="Id"/>,
/// established together at registration.
/// </summary>
public sealed class UserModel
{
    public Guid Id { get; }
    public string Username { get; }
    public string PasswordHash { get; private set; }

    private UserModel(Guid id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }

    public static UserModel Create(Guid id, string username, string passwordHash)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new UserModel(id, username.Trim(), passwordHash);
    }

    /// <summary>Rehydrates a user from persisted storage (no re-validation of stored data).</summary>
    public static UserModel FromPersistence(Guid id, string username, string passwordHash) =>
        new(id, username, passwordHash);

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }
}
