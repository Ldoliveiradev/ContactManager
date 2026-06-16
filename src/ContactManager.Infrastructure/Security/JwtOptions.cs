namespace ContactManager.Infrastructure.Security;

/// <summary>Strongly-typed JWT settings, bound from configuration ("Jwt" section).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}
