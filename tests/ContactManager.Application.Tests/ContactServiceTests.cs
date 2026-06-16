using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Models.Responses;
using ContactManager.Application.Contacts.Services;
using ContactManager.Domain.Entities;
using ContactManager.Domain.Repositories;
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
        var req = new CreateContactRequest("Ada", "ada@example.com", "+1-202-555-0100");

        var result = await _sut.CreateAsync(Owner, req);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Ada");
        _repo.Verify(r => r.AddAsync(
            It.Is<Contact>(c => c.UserId == Owner && c.Name == "Ada"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ReturnsFailure()
    {
        var req = new CreateContactRequest("Ada", "bad", null);

        var result = await _sut.CreateAsync(Owner, req);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        _repo.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Read ----

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByCaller_ReturnsSuccess()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(contact.Id));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(contact.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ReturnsFailure()
    {
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(contact.Id));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Contact?)null);

        var result = await _sut.GetByIdAsync(Owner, new GetContactRequest(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCallersContacts()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByUserAsync(
                Owner,
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Contact> { contact }, 1));

        var result = await _sut.GetAllAsync(Owner, new GetContactsRequest());

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    // ---- Update ----

    [Fact]
    public async Task UpdateAsync_WhenOwned_UpdatesAndPersists()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("Ada L.", "ada.l@example.com", "+1-202-555-0199"));

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Ada L.");
        _repo.Verify(r => r.UpdateAsync(
            It.Is<Contact>(c => c.Id == contact.Id && c.Name == "Ada L."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnother_ReturnsFailure()
    {
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("X", "x@example.com", null));

        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Delete ----

    [Fact]
    public async Task DeleteAsync_WhenOwned_ReturnsSuccess()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.DeleteAsync(Owner, new DeleteContactRequest(contact.Id));

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(contact.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnother_ReturnsFailure()
    {
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.DeleteAsync(Owner, new DeleteContactRequest(contact.Id));

        result.IsSuccess.Should().BeFalse();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
