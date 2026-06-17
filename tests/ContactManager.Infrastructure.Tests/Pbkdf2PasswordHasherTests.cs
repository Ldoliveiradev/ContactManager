using ContactManager.Infrastructure.Identity.Security;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new();

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        // Arrange
        // (no additional setup; _sut is initialised in the field initializer)

        // Act
        var hash = _sut.Hash("Secret123!");

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotContain("Secret123!");
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        // (no additional setup; random per-hash salt => identical passwords hash to different strings)

        // Act & Assert
        _sut.Hash("Secret123!").Should().NotBe(_sut.Hash("Secret123!"));
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _sut.Hash("Secret123!");

        // Act & Assert
        _sut.Verify("Secret123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _sut.Hash("Secret123!");

        // Act & Assert
        _sut.Verify("WrongPassword", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("a.b")]
    public void Verify_WithMalformedHash_ReturnsFalse(string malformed)
    {
        // Arrange
        // A corrupt/legacy stored hash must fail closed, not throw.

        // Act & Assert
        _sut.Verify("Secret123!", malformed).Should().BeFalse();
    }
}
