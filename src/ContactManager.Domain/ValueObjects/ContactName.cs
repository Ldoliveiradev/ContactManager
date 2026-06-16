namespace ContactManager.Domain.ValueObjects;

public sealed class ContactName : IEquatable<ContactName>
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ContactName(string value) => Value = value;

    public static ContactName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Contact name is required.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Contact name must be {MaxLength} characters or fewer.", nameof(value));

        return new ContactName(trimmed);
    }

    public bool Equals(ContactName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is ContactName n && Equals(n);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static bool operator ==(ContactName? left, ContactName? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ContactName? left, ContactName? right) => !(left == right);
}
