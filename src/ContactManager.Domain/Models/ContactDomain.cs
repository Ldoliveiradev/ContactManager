using System.Text.RegularExpressions;

namespace ContactManager.Domain.Models;

public sealed partial class ContactDomain
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }

    private ContactDomain(Guid id, Guid userId, string name, string email, string? phone)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Email = email;
        Phone = phone;
    }

    public static ContactDomain Create(Guid id, Guid userId, string name, string email, string? phone)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Contact id must not be empty.", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("Owner user id must not be empty.", nameof(userId));

        var (cleanName, cleanEmail, cleanPhone) = Normalize(name, email, phone);
        return new ContactDomain(id, userId, cleanName, cleanEmail, cleanPhone);
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

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
