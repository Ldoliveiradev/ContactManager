using ContactManager.Domain.Entities;
using ContactManager.Infrastructure.Persistence;
using ContactManager.Infrastructure.Tests.Database;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

[Collection("postgres")]
public class UserRepositoryTests
{
    private readonly PostgresTestFixture _db;
    private readonly UserRepository _sut;

    public UserRepositoryTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new UserRepository(PostgresTestFixture.TestConnectionString);
    }

    [SkippableFact]
    public async Task AddAsync_ThenGetByUsername_ReturnsPersistedUser()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        var user = User.Create(Guid.NewGuid(), "alice", "hashed-pw");
        await _sut.AddAsync(user);

        var loaded = await _sut.GetByUsernameAsync("alice");

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(user.Id);
        loaded.Username.Should().Be("alice");
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
    public async Task AddAsync_WithDuplicateUsername_Throws()
    {
        Skip.IfNot(_db.Available, "PostgreSQL test database not available.");
        await _db.ResetAsync();

        await _sut.AddAsync(User.Create(Guid.NewGuid(), "bob", "h1"));

        var act = () => _sut.AddAsync(User.Create(Guid.NewGuid(), "bob", "h2"));

        await act.Should().ThrowAsync<Exception>();
    }
}
