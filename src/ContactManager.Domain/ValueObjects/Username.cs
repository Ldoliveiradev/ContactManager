namespace ContactManager.Domain.ValueObjects;

public sealed class Username : IEquatable<Username>
{
    public const int MaxLength = 100;

    public string Value { get; }

    private Username(string value) => Value = value;

    public static Username From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username is required.", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Username must be {MaxLength} characters or fewer.", nameof(value));

        return new Username(trimmed);
    }

    public bool Equals(Username? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Username u && Equals(u);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static bool operator ==(Username? left, Username? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Username? left, Username? right) => !(left == right);
}
