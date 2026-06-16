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
        var email = Email.From($"  {input.ToUpperInvariant()}  ");
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
        var act = () => Email.From(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = Email.From("ada@example.com");
        var b = Email.From("ADA@EXAMPLE.COM");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        (Email.From("a@example.com") == Email.From("b@example.com")).Should().BeFalse();
    }
}

public class UsernameTests
{
    [Fact]
    public void From_TrimsWhitespace()
    {
        Username.From("  demo  ").Value.Should().Be("demo");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void From_Blank_Throws(string input)
    {
        var act = () => Username.From(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_AtMaxLength_Succeeds()
    {
        var value = new string('a', Username.MaxLength);
        Username.From(value).Value.Should().HaveLength(Username.MaxLength);
    }

    [Fact]
    public void From_OverMaxLength_Throws()
    {
        var value = new string('a', Username.MaxLength + 1);
        var act = () => Username.From(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        (Username.From("demo") == Username.From("demo")).Should().BeTrue();
        (Username.From("demo") != Username.From("other")).Should().BeTrue();
        Username.From("demo").GetHashCode().Should().Be(Username.From("demo").GetHashCode());
    }
}

public class FullNameTests
{
    [Fact]
    public void From_TrimsAndComposesValue()
    {
        var name = FullName.From("  Grace  ", "  Hopper  ");
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
        var act = () => FullName.From(first, last);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_OverMaxLength_Throws()
    {
        var first = new string('a', FullName.MaxLength);
        var act = () => FullName.From(first, "b");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = FullName.From("Grace", "Hopper");
        var b = FullName.From("Grace", "Hopper");
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
        ContactName.From("  Ada Lovelace  ").Value.Should().Be("Ada Lovelace");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void From_Blank_Throws(string input)
    {
        var act = () => ContactName.From(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_OverMaxLength_Throws()
    {
        var act = () => ContactName.From(new string('a', ContactName.MaxLength + 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        (ContactName.From("Ada") == ContactName.From("Ada")).Should().BeTrue();
        (ContactName.From("Ada") != ContactName.From("Grace")).Should().BeTrue();
    }
}

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+1-202-555-0100")]
    [InlineData("(202) 555-0100")]
    [InlineData("2025550100")]
    [InlineData("+44 20 7946 0958")]
    public void From_WithValidFormats_Succeeds(string input)
    {
        PhoneNumber.From(input).Value.Should().Be(input.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]                 // too short
    [InlineData("phone-number")]        // letters
    [InlineData("+1 (202) 555-0100 ext 99 extra long beyond twenty chars")] // too long
    public void From_WithInvalidFormats_Throws(string input)
    {
        var act = () => PhoneNumber.From(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        (PhoneNumber.From("2025550100") == PhoneNumber.From("2025550100")).Should().BeTrue();
        (PhoneNumber.From("2025550100") != PhoneNumber.From("2025550199")).Should().BeTrue();
        PhoneNumber.From("2025550100").GetHashCode()
            .Should().Be(PhoneNumber.From("2025550100").GetHashCode());
    }
}
