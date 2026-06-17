using ContactManager.Domain.ValueObjects;
using FluentAssertions;

namespace ContactManager.Domain.Tests;

public class EmailTests
{
    [Theory]
    [InlineData("ada@example.com")]
    [InlineData("a.b+tag@sub.domain.co")]
    public void From_WithValidEmail_NormalizesToLowercaseTrimmed(string input)
    {
        // Act
        var email = Email.From($"  {input.ToUpperInvariant()}  ");

        // Assert
        email.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@dot")]
    [InlineData("@no-local.com")]
    [InlineData("spaces in@email.com")]
    public void From_WithInvalidEmail_Throws(string input)
    {
        // Act
        var act = () => Email.From(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        // Arrange
        var a = Email.From("ada@example.com");
        var b = Email.From("ADA@EXAMPLE.COM");

        // Assert
        a.Should().Be(b);
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Act
        var result = Email.From("a@example.com") == Email.From("b@example.com");

        // Assert
        result.Should().BeFalse();
    }
}

public class FullNameTests
{
    [Fact]
    public void From_TrimsAndComposesValue()
    {
        // Act
        var name = FullName.From("  Grace  ", "  Hopper  ");

        // Assert
        name.FirstName.Should().Be("Grace");
        name.LastName.Should().Be("Hopper");
        name.Value.Should().Be("Grace Hopper");
    }

    [Theory]
    [InlineData("", "Hopper")]
    [InlineData("   ", "Hopper")]
    [InlineData("Grace", "")]
    [InlineData("Grace", "   ")]
    public void From_BlankPart_Throws(string first, string last)
    {
        // Act
        var act = () => FullName.From(first, last);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_OverMaxLength_Throws()
    {
        // Arrange
        var first = new string('a', FullName.MaxPartLength + 1);

        // Act
        var act = () => FullName.From(first, "b");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        // Arrange
        var a = FullName.From("Grace", "Hopper");
        var b = FullName.From("Grace", "Hopper");

        // Assert
        (a == b).Should().BeTrue();
        (a != FullName.From("Ada", "Lovelace")).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}

public class ContactNameTests
{
    [Fact]
    public void From_TrimsWhitespace()
    {
        // Act
        var name = ContactName.From("  Ada Lovelace  ");

        // Assert
        name.Value.Should().Be("Ada Lovelace");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void From_Blank_Throws(string input)
    {
        // Act
        var act = () => ContactName.From(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_OverMaxLength_Throws()
    {
        // Act
        var act = () => ContactName.From(new string('a', ContactName.MaxLength + 1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        // Act + Assert
        (ContactName.From("Ada") == ContactName.From("Ada")).Should().BeTrue();
        (ContactName.From("Ada") != ContactName.From("Grace")).Should().BeTrue();
    }
}

public class PhoneNumberTests
{
    [Theory]
    [InlineData("2025550100")]
    [InlineData("8001234567")]
    public void From_WithValidFormats_Succeeds(string input)
    {
        // Act
        var phone = PhoneNumber.From(input);

        // Assert
        phone.Value.Should().Be(input.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]                       // too short
    [InlineData("phone-number")]              // letters
    [InlineData("+1-202-555-0100")]           // formatted with dashes
    [InlineData("(202) 555-0100")]            // formatted with mask
    [InlineData("+44 20 7946 0958")]          // non-US
    public void From_WithInvalidFormats_Throws(string input)
    {
        // Act
        var act = () => PhoneNumber.From(input);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        // Act + Assert
        (PhoneNumber.From("2025550100") == PhoneNumber.From("2025550100")).Should().BeTrue();
        (PhoneNumber.From("2025550100") != PhoneNumber.From("2025550199")).Should().BeTrue();
        PhoneNumber.From("2025550100").GetHashCode()
            .Should().Be(PhoneNumber.From("2025550100").GetHashCode());
    }
}
