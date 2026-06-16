using Npgsql;

namespace ContactManager.Infrastructure.Data;

public abstract class BaseRepository(string connectionString)
{
    protected async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
