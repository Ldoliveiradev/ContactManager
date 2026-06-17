using System.Security.Claims;
using ContactManager.API.Extensions;
using FluentAssertions;

namespace ContactManager.API.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_FromNameIdentifierClaim_ReturnsId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, id.ToString());

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be(id);
    }

    [Fact]
    public void GetUserId_FromSubClaim_ReturnsId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var principal = PrincipalWith("sub", id.ToString());

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be(id);
    }

    [Fact]
    public void GetUserId_WithNoClaim_Throws()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var act = () => principal.GetUserId();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetUserId_WithMalformedClaim_Throws()
    {
        // Arrange
        var principal = PrincipalWith(ClaimTypes.NameIdentifier, "not-a-guid");

        // Act
        var act = () => principal.GetUserId();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static ClaimsPrincipal PrincipalWith(string claimType, string value) =>
        new(new ClaimsIdentity(new[] { new Claim(claimType, value) }));
}
