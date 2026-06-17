using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Services;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using FluentAssertions;
using Moq;

namespace ContactManager.Application.Tests;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _repo = new();
    private readonly ContactService _sut;

    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();

    public ContactServiceTests() => _sut = new ContactService(_repo.Object);

    // ---- Create ----

    [Fact]
    public async Task CreateAsync_PersistsContactOwnedByCaller()
    {
        // Arrange
        var req = new CreateContactRequest("Ada", "ada@example.com", "2025550100");

        // Act
        var result = await _sut.CreateAsync(Owner, req);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Ada");
        _repo.Verify(r => r.AddAsync(
            It.Is<ContactDomain>(c => c.AccountId == Owner && c.Name.Value == "Ada"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var req = new CreateContactRequest("Ada", "bad", null);

        // Act
        var result = await _sut.CreateAsync(Owner, req);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        _repo.Verify(r => r.AddAsync(It.IsAny<ContactDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Read ----

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByCaller_ReturnsSuccess()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(contact.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(contact.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ReturnsFailure()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(contact.Id));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Auth.Forbidden);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ContactDomain?)null);

        // Act
        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCallersContacts()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByAccountAsync(
                Owner,
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<ContactDomain> { contact }, 1));

        // Act
        var result = await _sut.GetAllAsync(Owner, new PaginationFilter());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_PassesSearchSortAndPagingToRepository()
    {
        // Arrange
        _repo.Setup(r => r.GetByAccountAsync(
                Owner, "ada", "email", true, 2, 25, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<ContactDomain>(), 0));

        var filter = new PaginationFilter(Search: "ada", SortBy: "email", SortDesc: true, Page: 2, PageSize: 25);

        // Act
        await _sut.GetAllAsync(Owner, filter);

        // Assert
        _repo.Verify(r => r.GetByAccountAsync(
            Owner, "ada", "email", true, 2, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ComputesPaginationMetadata()
    {
        // 15 total, page 1, size 6 => 3 pages, has next, no previous.
        // Arrange
        _repo.Setup(r => r.GetByAccountAsync(
                Owner, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                1, 6, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<ContactDomain>(), 15));

        var filter = new PaginationFilter(Page: 1, PageSize: 6);

        // Act
        var result = await _sut.GetAllAsync(Owner, filter);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(6);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    // ---- Update ----

    [Fact]
    public async Task UpdateAsync_WhenOwned_UpdatesAndPersists()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("Ada L.", "ada.l@example.com", "2025550199"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Ada L.");
        _repo.Verify(r => r.UpdateAsync(
            It.Is<ContactDomain>(c => c.Id == contact.Id && c.Name.Value == "Ada L."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnother_ReturnsFailure()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("Xyz", "x@example.com", null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Auth.Forbidden);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<ContactDomain>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Delete ----

    [Fact]
    public async Task DeleteAsync_WhenOwned_ReturnsSuccess()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.DeleteAsync(Owner, new DeleteContactRequest(contact.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(contact.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnother_ReturnsFailure()
    {
        // Arrange
        var contact = ContactDomain.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        // Act
        var result = await _sut.DeleteAsync(Owner, new DeleteContactRequest(contact.Id));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Auth.Forbidden);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
