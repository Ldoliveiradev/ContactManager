using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Data.Repositories;
using ContactManager.Infrastructure.Tests.Database;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

[Collection("postgres")]
public class AccountRepositoryTests
{
    private readonly PostgresTestFixture _db;
    private readonly AccountRepository _sut;

    public AccountRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new AccountRepository(PostgresTestFixture.TestConnectionString);
    }

    [Fact]
    public async Task AddAsync_ThenGetByUsername_ReturnsPersistedAccountDomain()
    {
        await PostgresTestFixture.ResetAsync();

        var account = AccountDomain.Create(Guid.NewGuid(), "alice", "Alice", "Smith", "alice@example.com", "hashed-pw");
        await _sut.AddAsync(account);

        var loaded = await _sut.GetByUsernameAsync("alice");

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(account.Id);
        loaded.Username.Value.Should().Be("alice");
        loaded.FullName.FirstName.Should().Be("Alice");
        loaded.FullName.LastName.Should().Be("Smith");
        loaded.Email.Value.Should().Be("alice@example.com");
        loaded.PasswordHash.Should().Be("hashed-pw");
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenMissing_ReturnsNull()
    {
        await PostgresTestFixture.ResetAsync();

        var loaded = await _sut.GetByUsernameAsync("nobody");

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistedAccountDomain()
    {
        await PostgresTestFixture.ResetAsync();

        var account = AccountDomain.Create(Guid.NewGuid(), "bob", "Bob", "Jones", "bob@example.com", "hashed-pw");
        await _sut.AddAsync(account);

        var loaded = await _sut.GetByIdAsync(account.Id);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(account.Id);
        loaded.Username.Value.Should().Be("bob");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateUsername_Throws()
    {
        await PostgresTestFixture.ResetAsync();

        await _sut.AddAsync(AccountDomain.Create(Guid.NewGuid(), "carol", "Carol", "White", "carol@example.com", "h1"));

        var act = () => _sut.AddAsync(AccountDomain.Create(Guid.NewGuid(), "carol", "Carol", "White", "carol2@example.com", "h2"));

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateAsync_PersistsProfileChanges()
    {
        await PostgresTestFixture.ResetAsync();
        var account = AccountDomain.Create(Guid.NewGuid(), "dave", "Dave", "Brown", "dave@example.com", "hash");
        await _sut.AddAsync(account);

        account.UpdateProfile("David", "Green", "david@example.com");
        await _sut.UpdateAsync(account);

        var loaded = await _sut.GetByIdAsync(account.Id);
        loaded!.FullName.FirstName.Should().Be("David");
        loaded.FullName.LastName.Should().Be("Green");
        loaded.Email.Value.Should().Be("david@example.com");
        // Username is immutable and unchanged.
        loaded.Username.Value.Should().Be("dave");
    }

    [Fact]
    public async Task UpdateAsync_PersistsNewPasswordHash()
    {
        await PostgresTestFixture.ResetAsync();
        var account = AccountDomain.Create(Guid.NewGuid(), "erin", "Erin", "Black", "erin@example.com", "old-hash");
        await _sut.AddAsync(account);

        account.UpdatePasswordHash("new-hash");
        await _sut.UpdateAsync(account);

        var loaded = await _sut.GetByIdAsync(account.Id);
        loaded!.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        await PostgresTestFixture.ResetAsync();

        (await _sut.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }
}
