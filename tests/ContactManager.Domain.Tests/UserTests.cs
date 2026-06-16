using ContactManager.Domain.Entities;
using FluentAssertions;

namespace ContactManager.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();

        var user = User.Create(id, "demo", "hashed-password");

        user.Id.Should().Be(id);
        user.Username.Should().Be("demo");
        user.PasswordHash.Should().Be("hashed-password");
    }

    [Fact]
    public void Create_TrimsUsername()
    {
        var user = User.Create(Guid.NewGuid(), "  demo  ", "hash");

        user.Username.Should().Be("demo");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankUsername_Throws(string? username)
    {
        var act = () => User.Create(Guid.NewGuid(), username!, "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankPasswordHash_Throws(string? hash)
    {
        var act = () => User.Create(Guid.NewGuid(), "demo", hash!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("passwordHash");
    }

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        var act = () => User.Create(Guid.Empty, "demo", "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }
}
