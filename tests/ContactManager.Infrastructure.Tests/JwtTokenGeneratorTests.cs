using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ContactManager.Domain.Entities;
using ContactManager.Infrastructure.Security;
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

    private readonly JwtTokenGenerator _sut = new(Options);

    [Fact]
    public void Generate_ProducesTokenWithUserIdAndUsernameClaims()
    {
        var user = User.Create(Guid.NewGuid(), "demo", "hash");

        var token = _sut.Generate(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Subject.Should().Be(user.Id.ToString());
        jwt.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "demo");
    }

    [Fact]
    public void Generate_TokenIsSignedAndValidatesAgainstConfiguredKey()
    {
        var user = User.Create(Guid.NewGuid(), "demo", "hash");
        var token = _sut.Generate(user);

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

        var act = () => new JwtSecurityTokenHandler()
            .ValidateToken(token, parameters, out _);

        act.Should().NotThrow();
        new JwtSecurityTokenHandler()
            .ValidateToken(token, parameters, out var validated);
        validated.Should().NotBeNull();
    }

    [Fact]
    public void Generate_TokenFailsValidationWithWrongKey()
    {
        var user = User.Create(Guid.NewGuid(), "demo", "hash");
        var token = _sut.Generate(user);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("a-completely-different-wrong-secret-key-32chars")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var act = () => new JwtSecurityTokenHandler()
            .ValidateToken(token, parameters, out _);

        act.Should().Throw<SecurityTokenInvalidSignatureException>();
    }
}
