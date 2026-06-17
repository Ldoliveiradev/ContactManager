using ContactManager.Domain.Models;
using FluentAssertions;

namespace ContactManager.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var account = AccountDomain.Create(id, "John", "Doe", "john@example.com");

        // Assert
        account.Id.Should().Be(id);
        account.FullName.FirstName.Should().Be("John");
        account.FullName.LastName.Should().Be("Doe");
        account.Email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void Create_NormalizesEmail()
    {
        // Act
        var account = AccountDomain.Create(Guid.NewGuid(), "John", "Doe", "  JOHN@Example.COM  ");

        // Assert
        account.Email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        // Act
        var act = () => AccountDomain.Create(Guid.Empty, "John", "Doe", "john@example.com");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        // Act
        var act = () => AccountDomain.Create(Guid.NewGuid(), "John", "Doe", "not-an-email");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_ChangesNameAndEmail()
    {
        // Arrange
        var account = AccountDomain.Create(Guid.NewGuid(), "John", "Doe", "john@example.com");

        // Act
        account.UpdateProfile("Jane", "Smith", "jane@example.com");

        // Assert
        account.FullName.FirstName.Should().Be("Jane");
        account.FullName.LastName.Should().Be("Smith");
        account.Email.Value.Should().Be("jane@example.com");
    }

    [Fact]
    public void UpdateProfile_WithInvalidEmail_Throws()
    {
        // Arrange
        var account = AccountDomain.Create(Guid.NewGuid(), "John", "Doe", "john@example.com");

        // Act
        var act = () => account.UpdateProfile("Jane", "Smith", "not-an-email");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
