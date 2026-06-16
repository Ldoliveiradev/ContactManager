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

    [SkippableFact]
    public async Task AddAsync_ThenGetByUsername_ReturnsPersistedAccountDomain()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

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

    [SkippableFact]
    public async Task GetByUsernameAsync_WhenMissing_ReturnsNull()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        var loaded = await _sut.GetByUsernameAsync("nobody");

        loaded.Should().BeNull();
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsPersistedAccountDomain()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        var account = AccountDomain.Create(Guid.NewGuid(), "bob", "Bob", "Jones", "bob@example.com", "hashed-pw");
        await _sut.AddAsync(account);

        var loaded = await _sut.GetByIdAsync(account.Id);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(account.Id);
        loaded.Username.Value.Should().Be("bob");
    }

    [SkippableFact]
    public async Task AddAsync_WithDuplicateUsername_Throws()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        await _sut.AddAsync(AccountDomain.Create(Guid.NewGuid(), "carol", "Carol", "White", "carol@example.com", "h1"));

        var act = () => _sut.AddAsync(AccountDomain.Create(Guid.NewGuid(), "carol", "Carol", "White", "carol2@example.com", "h2"));

        await act.Should().ThrowAsync<Exception>();
    }
}
