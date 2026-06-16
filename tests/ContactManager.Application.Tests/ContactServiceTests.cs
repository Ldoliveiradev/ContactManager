using ContactManager.Application.Abstractions;
using ContactManager.Application.Contacts;
using ContactManager.Application.Exceptions;
using ContactManager.Domain.Entities;
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

        result.Name.Should().Be("Ada");
        _repo.Verify(r => r.AddAsync(
            It.Is<Contact>(c => c.UserId == Owner && c.Name == "Ada"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ThrowsValidation()
    {
        var req = new CreateContactRequest("Ada", "bad", null);

        var act = () => _sut.CreateAsync(Owner, req);

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Read ----

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByCaller_ReturnsContact()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.GetByIdAsync(Owner, contact.Id);

        result.Id.Should().Be(contact.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        // IDOR protection: a contact owned by someone else must look like it does not exist.
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var act = () => _sut.GetByIdAsync(Owner, contact.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Contact?)null);

        var act = () => _sut.GetByIdAsync(Owner, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCallersContacts()
    {
        _repo.Setup(r => r.GetByUserAsync(Owner, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Contact>
             {
                 Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null)
             });

        var result = await _sut.GetAllAsync(Owner);

        result.Should().HaveCount(1);
        // Repository is queried scoped to the caller, never "get all contacts".
        _repo.Verify(r => r.GetByUserAsync(Owner, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- Update ----

    [Fact]
    public async Task UpdateAsync_WhenOwned_UpdatesAndPersists()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var result = await _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("Ada L.", "ada.l@example.com", "+1-202-555-0199"));

        result.Name.Should().Be("Ada L.");
        _repo.Verify(r => r.UpdateAsync(
            It.Is<Contact>(c => c.Id == contact.Id && c.Name == "Ada L."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnother_ThrowsNotFound()
    {
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var act = () => _sut.UpdateAsync(Owner, contact.Id,
            new UpdateContactRequest("X", "x@example.com", null));

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- Delete ----

    [Fact]
    public async Task DeleteAsync_WhenOwned_Deletes()
    {
        var contact = Contact.Create(Guid.NewGuid(), Owner, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        await _sut.DeleteAsync(Owner, contact.Id);

        _repo.Verify(r => r.DeleteAsync(contact.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnother_ThrowsNotFound()
    {
        var contact = Contact.Create(Guid.NewGuid(), Other, "Ada", "ada@example.com", null);
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);

        var act = () => _sut.DeleteAsync(Owner, contact.Id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
