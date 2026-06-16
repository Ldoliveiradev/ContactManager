using ContactManager.Infrastructure.Security;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new();

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        var hash = _sut.Hash("Secret123!");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotContain("Secret123!");
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Random per-hash salt => identical passwords hash to different strings.
        _sut.Hash("Secret123!").Should().NotBe(_sut.Hash("Secret123!"));
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("Secret123!");

        _sut.Verify("Secret123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("Secret123!");

        _sut.Verify("WrongPassword", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("a.b")]
    public void Verify_WithMalformedHash_ReturnsFalse(string malformed)
    {
        // A corrupt/legacy stored hash must fail closed, not throw.
        _sut.Verify("Secret123!", malformed).Should().BeFalse();
    }
}
