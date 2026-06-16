using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using Npgsql;

namespace ContactManager.Infrastructure.Data;

public sealed class AccountRepository(string connectionString)
    : BaseRepository(connectionString), IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, username, first_name, last_name, email, password_hash
            FROM accounts
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, username, first_name, last_name, email, password_hash
            FROM accounts
            WHERE username = @username
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(Account account, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO accounts (id, username, first_name, last_name, email, password_hash)
            VALUES (@id, @username, @firstName, @lastName, @email, @passwordHash)
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        BindAccount(cmd, account);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE accounts
            SET first_name = @firstName, last_name = @lastName, email = @email,
                password_hash = @passwordHash, updated_at = now()
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", account.Id);
        cmd.Parameters.AddWithValue("@firstName", account.FullName.FirstName);
        cmd.Parameters.AddWithValue("@lastName", account.FullName.LastName);
        cmd.Parameters.AddWithValue("@email", account.Email.Value);
        cmd.Parameters.AddWithValue("@passwordHash", account.PasswordHash);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void BindAccount(NpgsqlCommand cmd, Account account)
    {
        cmd.Parameters.AddWithValue("@id", account.Id);
        cmd.Parameters.AddWithValue("@username", account.Username.Value);
        cmd.Parameters.AddWithValue("@firstName", account.FullName.FirstName);
        cmd.Parameters.AddWithValue("@lastName", account.FullName.LastName);
        cmd.Parameters.AddWithValue("@email", account.Email.Value);
        cmd.Parameters.AddWithValue("@passwordHash", account.PasswordHash);
    }

    private static Account Map(NpgsqlDataReader reader) => Account.Create(
        reader.GetGuid(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.GetString(4),
        reader.GetString(5));
}
