namespace ContactManager.Domain.ValueObjects;

public sealed class FullName : IEquatable<FullName>
{
    public const int MinPartLength = 2;
    public const int MaxPartLength = 50;

    public string FirstName { get; }
    public string LastName { get; }
    public string Value => $"{FirstName} {LastName}";

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName From(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        var first = firstName.Trim();
        var last = lastName.Trim();

        if (first.Length < MinPartLength)
            throw new ArgumentException($"First name must be at least {MinPartLength} characters.", nameof(firstName));
        if (first.Length > MaxPartLength)
            throw new ArgumentException($"First name must be {MaxPartLength} characters or fewer.", nameof(firstName));

        if (last.Length < MinPartLength)
            throw new ArgumentException($"Last name must be at least {MinPartLength} characters.", nameof(lastName));
        if (last.Length > MaxPartLength)
            throw new ArgumentException($"Last name must be {MaxPartLength} characters or fewer.", nameof(lastName));

        return new FullName(first, last);
    }

    public bool Equals(FullName? other) =>
        other is not null && FirstName == other.FirstName && LastName == other.LastName;
    public override bool Equals(object? obj) => obj is FullName f && Equals(f);
    public override int GetHashCode() => HashCode.Combine(FirstName, LastName);
    public override string ToString() => Value;

    public static bool operator ==(FullName? left, FullName? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(FullName? left, FullName? right) => !(left == right);
}
