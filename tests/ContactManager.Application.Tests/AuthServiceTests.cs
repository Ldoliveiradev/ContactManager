using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using ContactManager.Application.Auth.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using FluentAssertions;
using Moq;

namespace ContactManager.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenGenerator> _tokens = new();

    private AuthService CreateSut() => new(_users.Object, _accounts.Object, _hasher.Object, _tokens.Object);

    // ---- Register ----

    [Fact]
    public async Task RegisterAsync_WithNewUsername_HashesPasswordAndPersists()
    {
        // Arrange
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);
        _accounts.Setup(r => r.ExistsByEmailAsync("demo@example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("Secret123!")).Returns("hashed");

        // Act
        var result = await CreateSut().RegisterAsync(
            new RegisterRequest("demo", "Test", "UserModel", "demo@example.com", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Username.Should().Be("demo");
        _users.Verify(r => r.AddAsync(
            It.Is<UserModel>(u => u.Username == "demo" && u.PasswordHash == "hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
        _accounts.Verify(r => r.AddAsync(
            It.Is<AccountDomain>(a => a.Email.Value == "demo@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsFailure()
    {
        // Arrange
        var existingUser = UserModel.Create(Guid.NewGuid(), "demo", "some-hash");
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(existingUser);

        // Act
        var result = await CreateSut().RegisterAsync(
            new RegisterRequest("demo", "Test", "UserModel", "demo@example.com", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already taken");
        _users.Verify(r => r.AddAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _accounts.Verify(r => r.AddAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        _users.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);
        _accounts.Setup(r => r.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        // Act
        var result = await CreateSut().RegisterAsync(
            new RegisterRequest("newuser", "Test", "User", "taken@example.com", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email already exists");
        _users.Verify(r => r.AddAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _accounts.Verify(r => r.AddAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public async Task RegisterAsync_WithInvalidInput_ReturnsFailure(string username, string password)
    {
        // Act
        var result = await CreateSut().RegisterAsync(
            new RegisterRequest(username, "Test", "UserModel", "demo@example.com", password));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    // ---- Login ----

    [Fact]
    public async Task LoginAsync_WithValidUsername_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserModel.FromPersistence(userId, "demo", "stored-hash");
        var account = AccountDomain.Create(userId, "Test", "UserModel", "demo@example.com");
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Secret123!", "stored-hash")).Returns(true);
        _accounts.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);
        _tokens.Setup(t => t.Generate(userId, "demo", "demo@example.com")).Returns("jwt-token");

        // Act
        var result = await CreateSut().LoginAsync(new LoginRequest("demo", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithValidEmail_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserModel.FromPersistence(userId, "demo", "stored-hash");
        var account = AccountDomain.Create(userId, "Test", "UserModel", "demo@example.com");
        _users.Setup(r => r.GetByUsernameAsync("demo@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);
        _users.Setup(r => r.GetByEmailAsync("demo@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Secret123!", "stored-hash")).Returns(true);
        _accounts.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(account);
        _tokens.Setup(t => t.Generate(userId, "demo", "demo@example.com")).Returns("jwt-token");

        // Act
        var result = await CreateSut().LoginAsync(new LoginRequest("demo@example.com", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var user = UserModel.FromPersistence(Guid.NewGuid(), "demo", "stored-hash");
        _users.Setup(r => r.GetByUsernameAsync("demo", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await CreateSut().LoginAsync(new LoginRequest("demo", "wrong"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ReturnsFailure()
    {
        // Arrange
        _users.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);

        // Act
        var result = await CreateSut().LoginAsync(new LoginRequest("ghost", "Secret123!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    // ---- ChangePassword ----

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_UpdatesHash()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserModel.FromPersistence(userId, "demo", "old-hash");
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("OldPass1!", "old-hash")).Returns(true);
        _hasher.Setup(h => h.Hash("NewPass1!")).Returns("new-hash");

        // Act
        var result = await CreateSut().ChangePasswordAsync(userId,
            new ChangePasswordRequest(userId, "OldPass1!", "NewPass1!", "NewPass1!"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _users.Verify(r => r.UpdateAsync(
            It.Is<UserModel>(u => u.PasswordHash == "new-hash"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ReturnsInvalidCurrentPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserModel.FromPersistence(userId, "demo", "old-hash");
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await CreateSut().ChangePasswordAsync(userId,
            new ChangePasswordRequest(userId, "WrongPass!", "NewPass1!", "NewPass1!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("incorrect");
        _users.Verify(r => r.UpdateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync((UserModel?)null);

        // Act
        var result = await CreateSut().ChangePasswordAsync(userId,
            new ChangePasswordRequest(userId, "OldPass1!", "NewPass1!", "NewPass1!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _users.Verify(r => r.UpdateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "NewPass1!")]
    [InlineData("   ", "NewPass1!")]
    [InlineData("OldPass1!", "")]
    [InlineData("OldPass1!", "short")]
    public async Task ChangePasswordAsync_WithInvalidInput_ReturnsFailure(string current, string newPwd)
    {
        // Act
        var result = await CreateSut().ChangePasswordAsync(Guid.NewGuid(),
            new ChangePasswordRequest(Guid.NewGuid(), current, newPwd, newPwd));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        _users.Verify(r => r.UpdateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
