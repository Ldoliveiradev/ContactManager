using System.Security.Claims;
using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Identity.Security;
using ContactManager.Infrastructure.Identity.Services;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class IdentityServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Secret = "this-is-a-sufficiently-long-test-signing-secret-32+chars",
        Issuer = "contactmanager-test",
        Audience = "contactmanager-test-clients",
        ExpiryMinutes = 60
    };

    private readonly IdentityService _sut =
        new(new Pbkdf2PasswordHasher(), new JwtTokenGenerator(Options));

    [Fact]
    public void HashPassword_ThenVerifyPassword_RoundTrips()
    {
        var hash = _sut.HashPassword("Secret123!");

        hash.Should().NotBeNullOrWhiteSpace();
        _sut.VerifyPassword("Secret123!", hash).Should().BeTrue();
        _sut.VerifyPassword("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyJwt()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "demo", "Demo", "User", "demo@example.com", "hash");

        var token = _sut.GenerateToken(account);

        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3); // header.payload.signature
    }

    [Fact]
    public void IsCurrentUser_WithMatchingNameIdentifierClaim_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, id.ToString());

        _sut.IsCurrentUser(id, principal).Should().BeTrue();
    }

    [Fact]
    public void IsCurrentUser_WithMatchingSubClaim_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var principal = PrincipalWith("sub", id.ToString());

        _sut.IsCurrentUser(id, principal).Should().BeTrue();
    }

    [Fact]
    public void IsCurrentUser_WithDifferentId_ReturnsFalse()
    {
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString());

        _sut.IsCurrentUser(Guid.NewGuid(), principal).Should().BeFalse();
    }

    [Fact]
    public void IsCurrentUser_WithMalformedClaim_ReturnsFalse()
    {
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, "not-a-guid");

        _sut.IsCurrentUser(Guid.NewGuid(), principal).Should().BeFalse();
    }

    [Fact]
    public void IsCurrentUser_WithNoIdentityClaim_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        _sut.IsCurrentUser(Guid.NewGuid(), principal).Should().BeFalse();
    }

    private static ClaimsPrincipal PrincipalWith(string claimType, string value) =>
        new(new ClaimsIdentity(new[] { new Claim(claimType, value) }));
}
