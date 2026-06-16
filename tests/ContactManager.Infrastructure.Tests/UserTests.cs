using ContactManager.Domain.Models;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class AccountDomainTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();

        var account = AccountDomain.Create(id, "demo", "John", "Doe", "john@example.com", "hashed-password");

        account.Id.Should().Be(id);
        account.Username.Value.Should().Be("demo");
        account.FullName.FirstName.Should().Be("John");
        account.FullName.LastName.Should().Be("Doe");
        account.Email.Value.Should().Be("john@example.com");
        account.PasswordHash.Should().Be("hashed-password");
    }

    [Fact]
    public void Create_TrimsUsernameAndNormalizesEmail()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "  demo  ", "John", "Doe", "  JOHN@Example.COM  ", "hash");

        account.Username.Value.Should().Be("demo");
        account.Email.Value.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankUsername_Throws(string? username)
    {
        var act = () => AccountDomain.Create(Guid.NewGuid(), username!, "John", "Doe", "john@example.com", "hash");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankPasswordHash_Throws(string? hash)
    {
        var act = () => AccountDomain.Create(Guid.NewGuid(), "demo", "John", "Doe", "john@example.com", hash!);

        act.Should().Throw<ArgumentException>().WithParameterName("passwordHash");
    }

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        var act = () => AccountDomain.Create(Guid.Empty, "demo", "John", "Doe", "john@example.com", "hash");

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void UpdateProfile_ChangesNameAndEmail()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "demo", "John", "Doe", "john@example.com", "hash");

        account.UpdateProfile("Jane", "Smith", "jane@example.com");

        account.FullName.FirstName.Should().Be("Jane");
        account.FullName.LastName.Should().Be("Smith");
        account.Email.Value.Should().Be("jane@example.com");
    }
}
