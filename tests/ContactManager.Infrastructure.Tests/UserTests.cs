using ContactManager.Infrastructure.Auth.Models;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();

        var user = UserModel.Create(id, "demo", "hashed-password");

        user.Id.Should().Be(id);
        user.Username.Should().Be("demo");
        user.PasswordHash.Should().Be("hashed-password");
    }

    [Fact]
    public void Create_TrimsUsername()
    {
        var user = UserModel.Create(Guid.NewGuid(), "  demo  ", "hash");

        user.Username.Should().Be("demo");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankUsername_Throws(string? username)
    {
        var act = () => UserModel.Create(Guid.NewGuid(), username!, "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankPasswordHash_Throws(string? hash)
    {
        var act = () => UserModel.Create(Guid.NewGuid(), "demo", hash!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("passwordHash");
    }

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        var act = () => UserModel.Create(Guid.Empty, "demo", "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }
}
