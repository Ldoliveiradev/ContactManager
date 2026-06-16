using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using FluentAssertions;
using Moq;

namespace ContactManager.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenGenerator> _tokens = new();

    private AuthService CreateSut() => new(_accounts.Object, _hasher.Object, _tokens.Object);

    // ---- Register ----

    [Fact]
    public async Task RegisterAsync_WithNewUsername_HashesPasswordAndPersists()
    {
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync((AccountDomain?)null);
        _hasher.Setup(h => h.Hash("Secret123!")).Returns("hashed");

        var result = await CreateSut().RegisterAsync(new RegisterRequest("demo", "Test", "User", "demo@example.com", "Secret123!"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Username.Should().Be("demo");
        _accounts.Verify(r => r.AddAsync(
            It.Is<AccountDomain>(u => u.Username.Value == "demo" && u.PasswordHash == "hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsFailure()
    {
        var existing = AccountDomain.Create(Guid.NewGuid(), "demo", "Test", "User", "demo@example.com", "h");
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        var result = await CreateSut().RegisterAsync(new RegisterRequest("demo", "Test", "User", "demo@example.com", "Secret123!"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already taken");
        _accounts.Verify(r => r.AddAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public async Task RegisterAsync_WithInvalidInput_ReturnsFailure(string username, string password)
    {
        var result = await CreateSut().RegisterAsync(new RegisterRequest(username, "Test", "User", "demo@example.com", password));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
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

        var result = await CreateSut().LoginAsync(new LoginRequest("demo", "Secret123!"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        var account = AccountDomain.Create(Guid.NewGuid(), "demo", "Test", "User", "demo@example.com", "stored-hash");
        _accounts.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(account);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await CreateSut().LoginAsync(new LoginRequest("demo", "wrong"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ReturnsFailure()
    {
        _accounts.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((AccountDomain?)null);

        var result = await CreateSut().LoginAsync(new LoginRequest("ghost", "Secret123!"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
