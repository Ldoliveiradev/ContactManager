using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Models;
using ContactManager.Domain.Exceptions;
using ContactManager.Infrastructure.Data.Base;
using Npgsql;

namespace ContactManager.Infrastructure.Data.Repositories;

/// <summary>
/// Npgsql-backed persistence for the authentication identity (the <c>users</c>
/// table). Implements the auth repository abstraction defined in the Application
/// layer.
/// </summary>
public sealed class UserRepository(string connectionString)
    : BaseRepository(connectionString), IUserRepository
{
    public async Task<UserModel?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, username, password_hash
            FROM users
            WHERE username = @username
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<UserModel?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = """
            SELECT u.id, u.username, u.password_hash
            FROM users u
            JOIN accounts a ON a.id = u.id
            WHERE lower(a.email) = lower(@email)
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<UserModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, username, password_hash
            FROM users
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(UserModel user, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO users (id, username, password_hash)
            VALUES (@id, @username, @passwordHash)
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateUsernameException(user.Username);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM users
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(UserModel user, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE users
            SET password_hash = @passwordHash, updated_at = now()
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static UserModel Map(NpgsqlDataReader reader) => UserModel.FromPersistence(
        reader.GetGuid(0),
        reader.GetString(1),
        reader.GetString(2));
}
