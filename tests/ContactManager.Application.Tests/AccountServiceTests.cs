using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using FluentAssertions;
using Moq;

namespace ContactManager.Application.Tests;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly AccountService _sut;

    private static readonly Guid OwnerId = Guid.NewGuid();

    public AccountServiceTests() => _sut = new AccountService(_repo.Object, _hasher.Object);

    private static AccountDomain MakeAccount() =>
        AccountDomain.Create(OwnerId, "demo", "John", "Doe", "john@example.com", "stored-hash");

    // ---- GetById ----

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsSuccess()
    {
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _sut.GetByIdAsync(OwnerId, new GetAccountRequest(OwnerId));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(OwnerId);
        result.Data.Username.Should().Be("demo");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((AccountDomain?)null);

        var result = await _sut.GetByIdAsync(OwnerId, new GetAccountRequest(OwnerId));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    // ---- UpdateProfile ----

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_UpdatesAndPersists()
    {
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _sut.UpdateProfileAsync(OwnerId,
            new UpdateAccountRequest("Jane", "Smith", "jane@example.com"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Jane");
        result.Data.Email.Should().Be("jane@example.com");
        _repo.Verify(r => r.UpdateAsync(
            It.Is<AccountDomain>(a => a.FullName.FirstName == "Jane"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "Doe", "john@example.com")]
    [InlineData("John", "", "john@example.com")]
    [InlineData("John", "Doe", "not-an-email")]
    public async Task UpdateProfileAsync_WithInvalidData_ReturnsFailure(
        string firstName, string lastName, string email)
    {
        var result = await _sut.UpdateProfileAsync(OwnerId,
            new UpdateAccountRequest(firstName, lastName, email));

        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- UpdatePassword ----

    [Fact]
    public async Task UpdatePasswordAsync_WithCorrectCurrentPassword_UpdatesHash()
    {
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _hasher.Setup(h => h.Verify("OldPass1!", "stored-hash")).Returns(true);
        _hasher.Setup(h => h.Hash("NewPass1!")).Returns("new-hash");

        var result = await _sut.UpdatePasswordAsync(OwnerId,
            new UpdatePasswordRequest("OldPass1!", "NewPass1!"));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(
            It.Is<AccountDomain>(a => a.PasswordHash == "new-hash"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithWrongCurrentPassword_ReturnsFailure()
    {
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await _sut.UpdatePasswordAsync(OwnerId,
            new UpdatePasswordRequest("WrongPass!", "NewPass1!"));

        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("OldPass1!", "short")]
    [InlineData("OldPass1!", "")]
    [InlineData("", "NewPass1!")]
    public async Task UpdatePasswordAsync_WithInvalidInput_ReturnsFailure(string current, string newPwd)
    {
        var result = await _sut.UpdatePasswordAsync(OwnerId,
            new UpdatePasswordRequest(current, newPwd));

        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
