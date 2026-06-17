using ContactManager.Application.Auth.Models;
using ContactManager.Infrastructure.Data.Repositories;
using ContactManager.Infrastructure.Tests.Database;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

[Collection("postgres")]
public class UserRepositoryIntegrationTests
{
    private readonly PostgresTestFixture _db;
    private readonly UserRepository _sut;

    public UserRepositoryIntegrationTests(PostgresTestFixture db)
    {
        _db = db;
        _sut = new UserRepository(PostgresTestFixture.TestConnectionString);
    }

    [Fact]
    public async Task AddAsync_ThenGetByUsername_ReturnsPersistedUser()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var id = Guid.NewGuid();
        var user = UserModel.Create(id, "alice", "hashed-pw");
        await _sut.AddAsync(user);

        // Act
        var loaded = await _sut.GetByUsernameAsync("alice");

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.Username.Should().Be("alice");
        loaded.PasswordHash.Should().Be("hashed-pw");
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenMissing_ReturnsNull()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();

        // Act
        var loaded = await _sut.GetByUsernameAsync("nobody");

        // Assert
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedUser()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var id = Guid.NewGuid();
        var user = UserModel.Create(id, "bob", "hashed-pw");
        await _sut.AddAsync(user);

        // Act
        var loaded = await _sut.GetByIdAsync(id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.Username.Should().Be("bob");
        loaded.PasswordHash.Should().Be("hashed-pw");
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
    public async Task UpdateAsync_PersistsNewPasswordHash()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        var id = Guid.NewGuid();
        var user = UserModel.Create(id, "carol", "old-hash");
        await _sut.AddAsync(user);

        user.ChangePassword("new-hash");

        // Act
        await _sut.UpdateAsync(user);

        // Assert
        var loaded = await _sut.GetByIdAsync(id);
        loaded!.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateUsername_Throws()
    {
        // Arrange
        await PostgresTestFixture.ResetAsync();
        await _sut.AddAsync(UserModel.Create(Guid.NewGuid(), "dave", "hash1"));

        // Act
        var act = () => _sut.AddAsync(UserModel.Create(Guid.NewGuid(), "dave", "hash2"));

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
