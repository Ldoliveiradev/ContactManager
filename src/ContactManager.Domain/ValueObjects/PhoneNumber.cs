using System.Text.RegularExpressions;

namespace ContactManager.Domain.ValueObjects;

public sealed partial class PhoneNumber : IEquatable<PhoneNumber>
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be blank. Use null to represent no phone.", nameof(value));

        var trimmed = value.Trim();

        if (!PhoneRegex().IsMatch(trimmed))
            throw new ArgumentException("Phone number format is invalid.", nameof(value));

        return new PhoneNumber(trimmed);
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && PhoneRegex().IsMatch(value.Trim());

    public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PhoneNumber p && Equals(p);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);

    // 10 raw digits — mask is a UI concern only
    [GeneratedRegex(@"^\d{10}$")]
    private static partial Regex PhoneRegex();
}
