using ContactManager.Application.Auth.Models;
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
    private readonly UserRepository _users;

    public AccountRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new AccountRepository(PostgresTestFixture.TestConnectionString);
        _users = new UserRepository(PostgresTestFixture.TestConnectionString);
    }

    /// <summary>
    /// Seeds the required users row for a given id, because accounts.id
    /// is a foreign key that references users.id.
    /// </summary>
    private async Task SeedUserAsync(Guid id, string username, string passwordHash = "hashed-pw")
    {
        var user = UserModel.Create(id, username, passwordHash);
        await _users.AddAsync(user);
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedAccountDomain()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        var id = Guid.NewGuid();
        await SeedUserAsync(id, "alice");
        var account = AccountDomain.Create(id, "Alice", "Smith", "alice@example.com");
        await _sut.AddAsync(account);

        // Act
        var loaded = await _sut.GetByIdAsync(id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.FullName.FirstName.Should().Be("Alice");
        loaded.FullName.LastName.Should().Be("Smith");
        loaded.Email.Value.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        // Act
        var loaded = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistedAccountDomain()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        var id = Guid.NewGuid();
        await SeedUserAsync(id, "bob");
        var account = AccountDomain.Create(id, "Bob", "Jones", "bob@example.com");
        await _sut.AddAsync(account);

        // Act
        var loaded = await _sut.GetByIdAsync(account.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(account.Id);
        loaded.FullName.FirstName.Should().Be("Bob");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateEmail_Throws()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await SeedUserAsync(id1, "carol1");
        await SeedUserAsync(id2, "carol2");
        await _sut.AddAsync(AccountDomain.Create(id1, "Carol", "White", "carol@example.com"));

        // Act
        var act = () => _sut.AddAsync(AccountDomain.Create(id2, "Carol", "White", "carol@example.com"));

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateAsync_PersistsProfileChanges()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var id = Guid.NewGuid();
        await SeedUserAsync(id, "dave");
        var account = AccountDomain.Create(id, "Dave", "Brown", "dave@example.com");
        await _sut.AddAsync(account);

        account.UpdateProfile("David", "Green", "david@example.com");

        // Act
        await _sut.UpdateAsync(account);

        // Assert
        var loaded = await _sut.GetByIdAsync(account.Id);
        loaded!.FullName.FirstName.Should().Be("David");
        loaded.FullName.LastName.Should().Be("Green");
        loaded.Email.Value.Should().Be("david@example.com");
    }
}
