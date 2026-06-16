using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Auth.Models;
using ContactManager.Infrastructure.Auth.Services;
using ContactManager.Infrastructure.Persistence;
using ContactManager.Infrastructure.Tests.Database;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

[Collection("postgres")]
public class ContactRepositoryTests
{
    private readonly PostgresTestFixture _db;
    private readonly ContactRepository _sut;
    private readonly UserRepository _users;

    public ContactRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new ContactRepository(PostgresTestFixture.TestConnectionString);
        _users = new UserRepository(PostgresTestFixture.TestConnectionString);
    }

    private async Task<Guid> SeedUserAsync(string username = "owner")
    {
        var user = UserModel.Create(Guid.NewGuid(), username, "hash");
        await _users.AddAsync(user);
        return user.Id;
    }

    [SkippableFact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedContact()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var owner = await SeedUserAsync();

        var contact = ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", "+1-202-555-0100");
        await _sut.AddAsync(contact);

        var loaded = await _sut.GetByIdAsync(contact.Id);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(contact.Id);
        loaded.UserId.Should().Be(owner);
        loaded.Name.Should().Be("Ada");
        loaded.Email.Should().Be("ada@example.com");
        loaded.Phone.Should().Be("+1-202-555-0100");
    }

    [SkippableFact]
    public async Task GetByUserAsync_ReturnsOnlyThatUsersContacts()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var owner = await SeedUserAsync("owner");
        var other = await SeedUserAsync("other");

        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Alan", "alan@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), other, "Grace", "grace@example.com", null));

        var (items, totalCount) = await _sut.GetByUserAsync(owner, null, null, false, 1, 10);

        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
        items.Select(c => c.UserId).Should().AllBeEquivalentTo(owner);
    }

    [SkippableFact]
    public async Task UpdateAsync_PersistsChanges()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var owner = await SeedUserAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", null);
        await _sut.AddAsync(contact);

        contact.Update("Ada L.", "ada.l@example.com", "+1-202-555-0199");
        await _sut.UpdateAsync(contact);

        var loaded = await _sut.GetByIdAsync(contact.Id);
        loaded!.Name.Should().Be("Ada L.");
        loaded.Email.Should().Be("ada.l@example.com");
        loaded.Phone.Should().Be("+1-202-555-0199");
    }

    [SkippableFact]
    public async Task DeleteAsync_RemovesContact()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var owner = await SeedUserAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", null);
        await _sut.AddAsync(contact);

        await _sut.DeleteAsync(contact.Id);

        (await _sut.GetByIdAsync(contact.Id)).Should().BeNull();
    }

    [SkippableFact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        (await _sut.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }
}
