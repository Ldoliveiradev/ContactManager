using System.IdentityModel.Tokens.Jwt;
using ContactManager.Infrastructure.Identity.Security;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace ContactManager.Infrastructure.Tests;

public class JwtTokenGeneratorTests
{
    private static readonly JwtOptions Options = new()
    {
        Secret = "this-is-a-sufficiently-long-test-signing-secret-32+chars",
        Issuer = "contactmanager-test",
        Audience = "contactmanager-test-clients",
        ExpiryMinutes = 60
    };

    private readonly JwtTokenGenerator _sut = new(Options, TimeProvider.System);

    [Fact]
    public void Generate_ProducesTokenWithIdUsernameAndEmailClaims()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string username = "demo";
        const string email = "demo@example.com";

        // Act
        var token = _sut.Generate(id, username, email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Subject.Should().Be(id.ToString());
        jwt.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == username);
        jwt.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
    }

    [Fact]
    public void Generate_TokenIsSignedAndValidatesAgainstConfiguredKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var token = _sut.Generate(id, "demo", "demo@example.com");

        // Assert
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(Options.Secret)),
            ValidateLifetime = true
        };

        var act = () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        act.Should().NotThrow();
        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);
        validated.Should().NotBeNull();
    }

    [Fact]
    public void Generate_TokenFailsValidationWithWrongKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var token = _sut.Generate(id, "demo", "demo@example.com");

        // Assert
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("a-completely-different-wrong-secret-key-32chars")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var act = () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        act.Should().Throw<SecurityTokenInvalidSignatureException>();
    }

    [Fact]
    public void Generate_SetsExpiryFromTheInjectedClock()
    {
        // Arrange — a fixed clock makes the token's expiry deterministic.
        var now = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var sut = new JwtTokenGenerator(Options, new FixedTimeProvider(now));

        // Act
        var token = sut.Generate(Guid.NewGuid(), "demo", "demo@example.com");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert — expiry is exactly ExpiryMinutes after the injected "now".
        var expected = now.UtcDateTime.AddMinutes(Options.ExpiryMinutes);
        jwt.ValidTo.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    /// <summary>A minimal TimeProvider test double returning a fixed instant.</summary>
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
