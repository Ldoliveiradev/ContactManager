using ContactManager.Infrastructure.Auth.Interfaces;
using ContactManager.Infrastructure.Auth.Models;
using Npgsql;

namespace ContactManager.Infrastructure.Auth.Services;

public sealed class UserRepository(string connectionString) : IUserRepository
{
    public async Task<UserModel?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, username, password_hash
            FROM users
            WHERE username = @username
            """;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return UserModel.Create(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2));
    }

    public async Task AddAsync(UserModel user, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO users (id, username, password_hash)
            VALUES (@id, @username, @passwordHash)
            """;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
