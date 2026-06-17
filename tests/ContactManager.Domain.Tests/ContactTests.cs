using ContactManager.Domain.Models;
using FluentAssertions;

namespace ContactManager.Domain.Tests;

public class ContactTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var contact = ContactDomain.Create(id, OwnerId, "Ada Lovelace", "ada@example.com", "2025550100");

        // Assert
        contact.Id.Should().Be(id);
        contact.AccountId.Should().Be(OwnerId);
        contact.Name.Value.Should().Be("Ada Lovelace");
        contact.Email.Value.Should().Be("ada@example.com");
        contact.Phone!.Value.Should().Be("2025550100");
    }

    [Fact]
    public void Create_TrimsNameAndNormalizesEmail()
    {
        // Act
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "  Ada  ", "  ADA@Example.COM  ", null);

        // Assert
        contact.Name.Value.Should().Be("Ada");
        contact.Email.Value.Should().Be("ada@example.com");
    }

    [Fact]
    public void Create_WithNullPhone_IsAllowed()
    {
        // Act
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        // Assert
        contact.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? name)
    {
        // Act
        var act = () => ContactDomain.Create(Guid.NewGuid(), OwnerId, name!, "ada@example.com", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        // Act
        var act = () => ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", email, null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Create_WithEmptyAccountId_Throws()
    {
        // Act
        var act = () => ContactDomain.Create(Guid.NewGuid(), Guid.Empty, "Ada", "ada@example.com", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("accountId");
    }

    [Fact]
    public void Create_WithEmptyContactId_Throws()
    {
        // Act
        var act = () => ContactDomain.Create(Guid.Empty, OwnerId, "Ada", "ada@example.com", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void Update_ChangesMutableFields()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        // Act
        contact.Update("Ada L.", "ada.l@example.com", "2025550199");

        // Assert
        contact.Name.Value.Should().Be("Ada L.");
        contact.Email.Value.Should().Be("ada.l@example.com");
        contact.Phone!.Value.Should().Be("2025550199");
    }

    [Fact]
    public void Update_WithInvalidEmail_Throws()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), OwnerId, "Ada", "ada@example.com", null);

        // Act
        var act = () => contact.Update("Ada", "bad-email", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

}
