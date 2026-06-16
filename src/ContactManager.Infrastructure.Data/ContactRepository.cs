using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;
using ContactManager.Domain.ValueObjects;
using Npgsql;
using NpgsqlTypes;

namespace ContactManager.Infrastructure.Data;

public sealed class ContactRepository(string connectionString)
    : BaseRepository(connectionString), IContactRepository
{
    private static readonly HashSet<string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase) { "name", "email", "phone", "created_at" };

    public async Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetByAccountAsync(
        Guid accountId, string? search, string? sortBy, bool sortDesc, int page, int pageSize,
        CancellationToken ct = default)
    {
        var column = AllowedSortColumns.Contains(sortBy ?? "") ? sortBy! : "name";
        var direction = sortDesc ? "DESC" : "ASC";
        var offset = (page - 1) * pageSize;

        var sql = $"""
            SELECT id, account_id, name, email, phone,
                   COUNT(*) OVER() AS total_count
            FROM contacts
            WHERE account_id = @accountId
              AND (@search IS NULL OR name ILIKE @search OR email ILIKE @search OR phone ILIKE @search)
            ORDER BY {column} {direction}
            LIMIT @pageSize OFFSET @offset
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@accountId", accountId);
        cmd.Parameters.Add(new NpgsqlParameter("@search", NpgsqlDbType.Text)
        {
            Value = string.IsNullOrWhiteSpace(search) ? DBNull.Value : $"%{search.Trim()}%"
        });
        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        cmd.Parameters.AddWithValue("@offset", offset);

        var items = new List<Contact>();
        var totalCount = 0;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(Map(reader));
            if (totalCount == 0)
                totalCount = reader.GetInt32(5);
        }

        return (items, totalCount);
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, account_id, name, email, phone
            FROM contacts
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(Contact contact, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO contacts (id, account_id, name, email, phone)
            VALUES (@id, @accountId, @name, @email, @phone)
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        BindContact(cmd, contact);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(Contact contact, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE contacts
            SET name = @name, email = @email, phone = @phone, updated_at = now()
            WHERE id = @id
            """;

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", contact.Id);
        cmd.Parameters.AddWithValue("@name", contact.Name.Value);
        cmd.Parameters.AddWithValue("@email", contact.Email.Value);
        cmd.Parameters.AddWithValue("@phone", (object?)contact.Phone?.Value ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM contacts WHERE id = @id";

        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void BindContact(NpgsqlCommand cmd, Contact contact)
    {
        cmd.Parameters.AddWithValue("@id", contact.Id);
        cmd.Parameters.AddWithValue("@accountId", contact.AccountId);
        cmd.Parameters.AddWithValue("@name", contact.Name.Value);
        cmd.Parameters.AddWithValue("@email", contact.Email.Value);
        cmd.Parameters.AddWithValue("@phone", (object?)contact.Phone?.Value ?? DBNull.Value);
    }

    private static Contact Map(NpgsqlDataReader reader) => Contact.Create(
        reader.GetGuid(0),
        reader.GetGuid(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4));
}
