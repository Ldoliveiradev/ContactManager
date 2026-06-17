using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Services;
using ContactManager.Application.Common;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using FluentAssertions;
using Moq;

namespace ContactManager.Application.Tests;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repo = new();
    private readonly AccountService _sut;

    private static readonly Guid OwnerId = Guid.NewGuid();

    public AccountServiceTests() => _sut = new AccountService(_repo.Object);

    private static AccountDomain MakeAccount() =>
        AccountDomain.Create(OwnerId, "John", "Doe", "john@example.com");

    // ---- GetById ----

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsSuccess()
    {
        // Arrange
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        // Act
        var result = await _sut.GetByIdAsync(OwnerId, new GetAccountRequest(OwnerId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(OwnerId);
        result.Data.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((AccountDomain?)null);

        // Act
        var result = await _sut.GetByIdAsync(OwnerId, new GetAccountRequest(OwnerId));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetByIdAsync_WhenRequestedAccountDoesNotMatchCaller_ReturnsForbidden()
    {
        // Act
        var result = await _sut.GetByIdAsync(OwnerId, new GetAccountRequest(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Auth.Forbidden);
        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- UpdateProfile ----

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_UpdatesAndPersists()
    {
        // Arrange
        var account = MakeAccount();
        _repo.Setup(r => r.GetByIdAsync(OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        // Act
        var result = await _sut.UpdateProfileAsync(OwnerId,
            new UpdateAccountRequest("Jane", "Smith", "jane@example.com"));

        // Assert
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
        // Act
        var result = await _sut.UpdateProfileAsync(OwnerId,
            new UpdateAccountRequest(firstName, lastName, email));

        // Assert
        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<AccountDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
