using ContactManager.Application.Abstractions;
using ContactManager.Domain.Entities;
using Npgsql;

namespace ContactManager.Infrastructure.Persistence;

/// <summary>
/// Contact persistence with hand-written, parameterized SQL via Npgsql (raw ADO.NET).
/// No ORM — per the exercise constraint. All queries are parameterized (injection-safe).
/// </summary>
public sealed class ContactRepository(string connectionString) : IContactRepository
{
    public async Task<IReadOnlyList<Contact>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, name, email, phone
            FROM contacts
            WHERE user_id = @userId
            ORDER BY name
            """;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var results = new List<Contact>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(Map(reader));

        return results;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, name, email, phone
            FROM contacts
            WHERE id = @id
            """;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(Contact contact, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO contacts (id, user_id, name, email, phone)
            VALUES (@id, @userId, @name, @email, @phone)
            """;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
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

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", contact.Id);
        cmd.Parameters.AddWithValue("@name", contact.Name);
        cmd.Parameters.AddWithValue("@email", contact.Email);
        cmd.Parameters.AddWithValue("@phone", (object?)contact.Phone ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM contacts WHERE id = @id";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void BindContact(NpgsqlCommand cmd, Contact contact)
    {
        cmd.Parameters.AddWithValue("@id", contact.Id);
        cmd.Parameters.AddWithValue("@userId", contact.UserId);
        cmd.Parameters.AddWithValue("@name", contact.Name);
        cmd.Parameters.AddWithValue("@email", contact.Email);
        cmd.Parameters.AddWithValue("@phone", (object?)contact.Phone ?? DBNull.Value);
    }

    private static Contact Map(NpgsqlDataReader reader) => Contact.Create(
        reader.GetGuid(0),
        reader.GetGuid(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4));
}
