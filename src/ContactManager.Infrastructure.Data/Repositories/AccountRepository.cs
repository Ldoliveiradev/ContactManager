using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Exceptions;
using ContactManager.Domain.Models;
using ContactManager.Infrastructure.Data.Base;
using Npgsql;

namespace ContactManager.Infrastructure.Data.Repositories;

public sealed class AccountRepository(string connectionString)
    : BaseRepository(connectionString), IAccountRepository
{
    public async Task<AccountDomain?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, first_name, last_name, email
            FROM accounts
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(AccountDomain account, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO accounts (id, first_name, last_name, email)
            VALUES (@id, @firstName, @lastName, @email)
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        BindAccount(cmd, account);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateAccountEmailException(account.Email.Value);
        }
    }

    public async Task UpdateAsync(AccountDomain account, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE accounts
            SET first_name = @firstName, last_name = @lastName, email = @email, updated_at = now()
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        BindAccount(cmd, account);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = """
            SELECT 1 FROM accounts WHERE lower(email) = lower(@email) LIMIT 1
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);
        return await cmd.ExecuteScalarAsync(ct) is not null;
    }

    private static void BindAccount(NpgsqlCommand cmd, AccountDomain account)
    {
        cmd.Parameters.AddWithValue("@id", account.Id);
        cmd.Parameters.AddWithValue("@firstName", account.FullName.FirstName);
        cmd.Parameters.AddWithValue("@lastName", account.FullName.LastName);
        cmd.Parameters.AddWithValue("@email", account.Email.Value);
    }

    private static AccountDomain Map(NpgsqlDataReader reader) => AccountDomain.Create(
        reader.GetGuid(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3));
}
