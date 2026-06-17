using Npgsql;

namespace ContactManager.Infrastructure.Data.Base;

public abstract class BaseRepository(string connectionString)
{
    protected async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
