using System.Text.RegularExpressions;

namespace ContactManager.Domain.Entities;

/// <summary>
/// A contact owned by a <see cref="User"/>. Created and mutated only through methods that
/// enforce its invariants (owner present, name required, email required and well-formed).
/// </summary>
public sealed partial class Contact
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }

    private Contact(Guid id, Guid userId, string name, string email, string? phone)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Email = email;
        Phone = phone;
    }

    public static Contact Create(Guid id, Guid userId, string name, string email, string? phone)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Contact id must not be empty.", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("Owner user id must not be empty.", nameof(userId));

        var (cleanName, cleanEmail, cleanPhone) = Normalize(name, email, phone);
        return new Contact(id, userId, cleanName, cleanEmail, cleanPhone);
    }

    public void Update(string name, string email, string? phone)
    {
        var (cleanName, cleanEmail, cleanPhone) = Normalize(name, email, phone);
        Name = cleanName;
        Email = cleanEmail;
        Phone = cleanPhone;
    }

    private static (string Name, string Email, string? Phone) Normalize(string name, string email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex().IsMatch(email.Trim()))
            throw new ArgumentException("A valid email is required.", nameof(email));

        var cleanPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        return (name.Trim(), email.Trim(), cleanPhone);
    }

    // Pragmatic email check: non-empty local part, "@", domain with a dot and a TLD.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
