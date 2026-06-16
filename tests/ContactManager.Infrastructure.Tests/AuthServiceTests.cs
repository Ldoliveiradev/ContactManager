using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Identity.Exceptions;
using ContactManager.Infrastructure.Identity.Interfaces;
using ContactManager.Infrastructure.Identity.Models;
using ContactManager.Infrastructure.Identity.Security;
using ContactManager.Infrastructure.Identity.Services;
using ContactManager.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ContactManager.Infrastructure.Tests;

public class AuthServiceTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();

    private AuthService CreateSut() => new(_accounts.Object, _hasher.Object, _tokens.Object);

    // ---- Register ----

    [Fact]
    public async Task RegisterAsync_WithNewUsername_HashesPasswordAndPersists()
    {
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync((AccountDomain?)null);
        _hasher.Setup(h => h.Hash("Secret123!")).Returns("hashed");

        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterRequest("demo", "Test", "User", "demo@example.com", "Secret123!"));

        result.Username.Should().Be("demo");
        _accounts.Verify(r => r.AddAsync(
            It.Is<AccountDomain>(u => u.Username.Value == "demo" && u.PasswordHash == "hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ThrowsConflict()
    {
        var existing = AccountDomain.Create(Guid.NewGuid(), "demo", "Test", "User", "demo@example.com", "h");
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        var sut = CreateSut();
        var act = () => sut.RegisterAsync(new RegisterRequest("demo", "Test", "User", "demo@example.com", "Secret123!"));

        await act.Should().ThrowAsync<UsernameAlreadyExistsException>();
        _accounts.Verify(r => r.AddAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public async Task RegisterAsync_WithInvalidInput_ThrowsValidation(string username, string password)
    {
        var sut = CreateSut();
        var act = () => sut.RegisterAsync(new RegisterRequest(username, "Test", "User", "demo@example.com", password));

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ---- Login ----

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "demo", "Test", "User", "demo@example.com", "stored-hash");
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(account);
        _hasher.Setup(h => h.Verify("Secret123!", "stored-hash")).Returns(true);
        _tokens.Setup(t => t.Generate(account)).Returns("jwt-token");

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest("demo", "Secret123!"));

        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "demo", "Test", "User", "demo@example.com", "stored-hash");
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(account);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var sut = CreateSut();
        var act = () => sut.LoginAsync(new LoginRequest("demo", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ThrowsInvalidCredentials()
    {
        _accounts.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((AccountDomain?)null);

        var sut = CreateSut();
        var act = () => sut.LoginAsync(new LoginRequest("ghost", "Secret123!"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
