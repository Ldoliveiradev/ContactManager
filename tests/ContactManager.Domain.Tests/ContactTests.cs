using ContactManager.Domain.Models;
using FluentAssertions;

namespace ContactManager.Domain.Tests;

public class ContactTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();

        var contact = ContactDomain.Create(id, OwnerId, "Ada Lovelace", "ada@example.com", "+1-202-555-0100");

        contact.Id.Should().Be(id);
        contact.UserId.Should().Be(OwnerId);
        contact.Name.Should().Be("Ada Lovelace");
        contact.Email.Should().Be("ada@example.com");
        contact.Phone.Should().Be("+1-202-555-0100");
    }

    [Fact]
    public void Create_TrimsNameAndEmail()
    {
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "  Ada  ", "  ada@example.com  ", null);

        contact.Name.Should().Be("Ada");
        contact.Email.Should().Be("ada@example.com");
    }

    [Fact]
    public void Create_WithNullPhone_IsAllowed()
    {
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        contact.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? name)
    {
        var act = () => ContactDomain.Create(Guid.NewGuid(), OwnerId, name!, "ada@example.com", null);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        var act = () => ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", email, null);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void Create_WithEmptyOwner_Throws()
    {
        var act = () => ContactDomain.Create(Guid.NewGuid(), Guid.Empty, "Ada", "ada@example.com", null);

        act.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Fact]
    public void Update_ChangesMutableFields()
    {
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        contact.Update("Ada L.", "ada.l@example.com", "+1-202-555-0199");

        contact.Name.Should().Be("Ada L.");
        contact.Email.Should().Be("ada.l@example.com");
        contact.Phone.Should().Be("+1-202-555-0199");
    }

    [Fact]
    public void Update_WithInvalidEmail_Throws()
    {
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        var act = () => contact.Update("Ada", "bad-email", null);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }
}
