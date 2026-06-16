namespace ContactManager.Infrastructure.Auth.Models;

public sealed class UserModel
{
    public Guid Id { get; }
    public string Username { get; }
    public string PasswordHash { get; }

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
}
