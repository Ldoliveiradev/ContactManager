using System.Security.Claims;
using ContactManager.API.Extensions;
using FluentAssertions;

namespace ContactManager.API.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_FromNameIdentifierClaim_ReturnsId()
    {
        var id = Guid.NewGuid();
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, id.ToString());

        principal.GetUserId().Should().Be(id);
    }

    [Fact]
    public void GetUserId_FromSubClaim_ReturnsId()
    {
        var id = Guid.NewGuid();
        var principal = PrincipalWith("sub", id.ToString());

        principal.GetUserId().Should().Be(id);
    }

    [Fact]
    public void GetUserId_WithNoClaim_Throws()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var act = () => principal.GetUserId();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetUserId_WithMalformedClaim_Throws()
    {
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, "not-a-guid");

        var act = () => principal.GetUserId();

        act.Should().Throw<InvalidOperationException>();
    }

    private static ClaimsPrincipal PrincipalWith(string claimType, string value) =>
        new(new ClaimsIdentity(new[] { new Claim(claimType, value) }));
}
