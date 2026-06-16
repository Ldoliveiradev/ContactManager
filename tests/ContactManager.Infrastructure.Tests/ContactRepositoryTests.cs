using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Data.Repositories;
using ContactManager.Infrastructure.Tests.Database;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

[Collection("postgres")]
public class ContactRepositoryTests
{
    private readonly PostgresTestFixture _db;
    private readonly ContactRepository _sut;
    private readonly AccountRepository _accounts;

    public ContactRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new ContactRepository(PostgresTestFixture.TestConnectionString);
        _accounts = new AccountRepository(PostgresTestFixture.TestConnectionString);
    }

    private async Task<Guid> SeedAccountAsync(string username = "owner")
    {
        var account = AccountDomain.Create(Guid.NewGuid(), username, "Test", "User", $"{username}@example.com", "hash");
        await _accounts.AddAsync(account);
        return account.Id;
    }

    [SkippableFact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedContact()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var accountId = await SeedAccountAsync();

        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", "+1-202-555-0100");
        await _sut.AddAsync(contact);

        var loaded = await _sut.GetByIdAsync(contact.Id);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(contact.Id);
        loaded.AccountId.Should().Be(accountId);
        loaded.Name.Value.Should().Be("Ada");
        loaded.Email.Value.Should().Be("ada@example.com");
        loaded.Phone!.Value.Should().Be("+1-202-555-0100");
    }

    [SkippableFact]
    public async Task GetByAccountAsync_ReturnsOnlyThatAccountsContacts()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var owner = await SeedAccountAsync("owner");
        var other = await SeedAccountAsync("other");

        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Alan", "alan@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), other, "Grace", "grace@example.com", null));

        var (items, totalCount) = await _sut.GetByAccountAsync(owner, null, null, false, 1, 10);

        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
        items.Select(c => c.AccountId).Should().AllBeEquivalentTo(owner);
    }

    [SkippableFact]
    public async Task UpdateAsync_PersistsChanges()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var accountId = await SeedAccountAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", null);
        await _sut.AddAsync(contact);

        contact.Update("Ada L.", "ada.l@example.com", "+1-202-555-0199");
        await _sut.UpdateAsync(contact);

        var loaded = await _sut.GetByIdAsync(contact.Id);
        loaded!.Name.Value.Should().Be("Ada L.");
        loaded.Email.Value.Should().Be("ada.l@example.com");
        loaded.Phone!.Value.Should().Be("+1-202-555-0199");
    }

    [SkippableFact]
    public async Task DeleteAsync_RemovesContact()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();
        var accountId = await SeedAccountAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", null);
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
