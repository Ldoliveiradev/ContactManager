using ContactManager.Domain.Exceptions;
using ContactManager.Infrastructure.Auth.Interfaces;
using ContactManager.Infrastructure.Auth.Models;
using ContactManager.Infrastructure.Auth.Security;
using ContactManager.Infrastructure.Auth.Services;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ContactManager.Infrastructure.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();

    private AuthService CreateSut() => new(_users.Object, _hasher.Object, _tokens.Object);

    // ---- Register ----

    [Fact]
    public async Task RegisterAsync_WithNewUsername_HashesPasswordAndPersists()
    {
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);
        _hasher.Setup(h => h.Hash("Secret123!")).Returns("hashed");

        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterRequest("demo", "Secret123!"));

        result.Username.Should().Be("demo");
        _users.Verify(r => r.AddAsync(
            It.Is<UserModel>(u => u.Username == "demo" && u.PasswordHash == "hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ThrowsConflict()
    {
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(UserModel.Create(Guid.NewGuid(), "demo", "h"));

        var sut = CreateSut();
        var act = () => sut.RegisterAsync(new RegisterRequest("demo", "Secret123!"));

        await act.Should().ThrowAsync<UsernameAlreadyExistsException>();
        _users.Verify(r => r.AddAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public async Task RegisterAsync_WithInvalidInput_ThrowsValidation(string username, string password)
    {
        var sut = CreateSut();
        var act = () => sut.RegisterAsync(new RegisterRequest(username, password));

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ---- Login ----

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = UserModel.Create(Guid.NewGuid(), "demo", "stored-hash");
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Secret123!", "stored-hash")).Returns(true);
        _tokens.Setup(t => t.Generate(user)).Returns("jwt-token");

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest("demo", "Secret123!"));

        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var user = UserModel.Create(Guid.NewGuid(), "demo", "stored-hash");
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var sut = CreateSut();
        var act = () => sut.LoginAsync(new LoginRequest("demo", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ThrowsInvalidCredentials()
    {
        _users.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);

        var sut = CreateSut();
        var act = () => sut.LoginAsync(new LoginRequest("ghost", "Secret123!"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
