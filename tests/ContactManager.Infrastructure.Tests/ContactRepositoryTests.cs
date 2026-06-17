using ContactManager.Application.Auth.Models;
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
    private readonly UserRepository _users;

    public ContactRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new ContactRepository(PostgresTestFixture.TestConnectionString);
        _accounts = new AccountRepository(PostgresTestFixture.TestConnectionString);
        _users = new UserRepository(PostgresTestFixture.TestConnectionString);
    }

    /// <summary>
    /// Seeds a users row and a matching accounts row (shared id), then returns the id.
    /// Accounts FK-references users, so the user row must be inserted first.
    /// </summary>
    private async Task<Guid> SeedAccountAsync(string username = "owner")
    {
        var id = Guid.NewGuid();
        await _users.AddAsync(UserModel.Create(id, username, "hashed-pw"));
        var account = AccountDomain.Create(id, "Test", "UserModel", $"{username}@example.com");
        await _accounts.AddAsync(account);
        return id;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedContact()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var accountId = await SeedAccountAsync();

        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", "2025550100");
        await _sut.AddAsync(contact);

        // Act
        var loaded = await _sut.GetByIdAsync(contact.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(contact.Id);
        loaded.AccountId.Should().Be(accountId);
        loaded.Name.Value.Should().Be("Ada");
        loaded.Email.Value.Should().Be("ada@example.com");
        loaded.Phone!.Value.Should().Be("2025550100");
    }

    [Fact]
    public async Task GetByAccountAsync_ReturnsOnlyThatAccountsContacts()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var owner = await SeedAccountAsync("owner");
        var other = await SeedAccountAsync("other");

        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "ada@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Alan", "alan@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), other, "Grace", "grace@example.com", null));

        // Act
        var (items, totalCount) = await _sut.GetByAccountAsync(owner, null, null, false, 1, 10);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
        items.Select(c => c.AccountId).Should().AllBeEquivalentTo(owner);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var accountId = await SeedAccountAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", null);
        await _sut.AddAsync(contact);

        contact.Update("Ada L.", "ada.l@example.com", "2025550199");

        // Act
        await _sut.UpdateAsync(contact);

        // Assert
        var loaded = await _sut.GetByIdAsync(contact.Id);
        loaded!.Name.Value.Should().Be("Ada L.");
        loaded.Email.Value.Should().Be("ada.l@example.com");
        loaded.Phone!.Value.Should().Be("2025550199");
    }

    [Fact]
    public async Task DeleteAsync_RemovesContact()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var accountId = await SeedAccountAsync();
        var contact = ContactDomain.Create(Guid.NewGuid(), accountId, "Ada", "ada@example.com", null);
        await _sut.AddAsync(contact);

        // Act
        await _sut.DeleteAsync(contact.Id);

        // Assert
        (await _sut.GetByIdAsync(contact.Id)).Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        // Act & Assert
        (await _sut.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetByAccountAsync_SearchFiltersByNameEmailOrPhone()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var owner = await SeedAccountAsync("search-owner");
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada Lovelace", "ada@example.com", "2025550100"));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Alan Turing", "alan@bletchley.uk", null));

        // Act & Assert — match by name.
        var (byName, nameTotal) = await _sut.GetByAccountAsync(owner, "lovelace", null, false, 1, 10);
        byName.Should().ContainSingle().Which.Name.Value.Should().Be("Ada Lovelace");
        nameTotal.Should().Be(1);

        // Act & Assert — match by email domain.
        var (byEmail, _) = await _sut.GetByAccountAsync(owner, "bletchley", null, false, 1, 10);
        byEmail.Should().ContainSingle().Which.Name.Value.Should().Be("Alan Turing");

        // Act & Assert — match by phone fragment.
        var (byPhone, _) = await _sut.GetByAccountAsync(owner, "5550100", null, false, 1, 10);
        byPhone.Should().ContainSingle().Which.Name.Value.Should().Be("Ada Lovelace");

        // Act & Assert — no match.
        var (none, noneTotal) = await _sut.GetByAccountAsync(owner, "zzzznomatch", null, false, 1, 10);
        none.Should().BeEmpty();
        noneTotal.Should().Be(0);
    }

    [Fact]
    public async Task GetByAccountAsync_SortsByNameAscendingAndDescending()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var owner = await SeedAccountAsync("sort-owner");
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Charlie", "c@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "a@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Barbara", "b@example.com", null));

        // Act & Assert — ascending.
        var (asc, _) = await _sut.GetByAccountAsync(owner, null, "name", false, 1, 10);
        asc.Select(c => c.Name.Value).Should().ContainInOrder("Ada", "Barbara", "Charlie");

        // Act & Assert — descending.
        var (desc, _) = await _sut.GetByAccountAsync(owner, null, "name", true, 1, 10);
        desc.Select(c => c.Name.Value).Should().ContainInOrder("Charlie", "Barbara", "Ada");
    }

    [Theory]
    [InlineData("name; DROP TABLE contacts; --")]
    [InlineData("(SELECT 1)")]
    [InlineData("unknown_column")]
    public async Task GetByAccountAsync_InvalidSortColumn_FallsBackToNameSafely(string maliciousSort)
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var owner = await SeedAccountAsync("inject-owner");
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Charlie", "c@example.com", null));
        await _sut.AddAsync(ContactDomain.Create(Guid.NewGuid(), owner, "Ada", "a@example.com", null));

        // Act
        // The allow-list must reject the injection and fall back to "name ASC"
        // without throwing or executing arbitrary SQL.
        var (items, total) = await _sut.GetByAccountAsync(owner, null, maliciousSort, false, 1, 10);

        // Assert
        total.Should().Be(2);
        items.Select(c => c.Name.Value).Should().ContainInOrder("Ada", "Charlie");

        // The table still exists and is intact.
        (await _sut.GetByAccountAsync(owner, null, null, false, 1, 10)).TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByAccountAsync_PaginatesAcrossPages()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var owner = await SeedAccountAsync("page-owner");
        for (var i = 0; i < 15; i++)
            await _sut.AddAsync(ContactDomain.Create(
                Guid.NewGuid(), owner, $"Contact {i:D2}", $"c{i:D2}@example.com", null));

        // Act & Assert — page 1.
        var (page1, total1) = await _sut.GetByAccountAsync(owner, null, "name", false, 1, 6);
        page1.Should().HaveCount(6);
        total1.Should().Be(15);
        page1[0].Name.Value.Should().Be("Contact 00");

        // Act & Assert — page 2.
        var (page2, total2) = await _sut.GetByAccountAsync(owner, null, "name", false, 2, 6);
        page2.Should().HaveCount(6);
        total2.Should().Be(15);
        page2[0].Name.Value.Should().Be("Contact 06");

        // Act & Assert — page 3 (partial).
        var (page3, total3) = await _sut.GetByAccountAsync(owner, null, "name", false, 3, 6);
        page3.Should().HaveCount(3);
        total3.Should().Be(15);
        page3[0].Name.Value.Should().Be("Contact 12");
    }
}
