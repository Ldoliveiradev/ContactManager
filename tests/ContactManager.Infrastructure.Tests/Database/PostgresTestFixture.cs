using System.Net.Sockets;
using Npgsql;

namespace ContactManager.Infrastructure.Tests.Database;

/// <summary>
/// Spins the integration tests against a dedicated <c>contactmanager_test</c> database on the
/// local Docker Postgres. The database and schema are created once per test run; individual
/// tests clean the tables they use. Skipped gracefully if Postgres is not reachable so the
/// suite still runs (e.g. in CI without a DB) without failing.
/// </summary>
public sealed class PostgresTestFixture : IAsyncLifetime
{
    // Matches docker-compose (host port 5433). Admin connects to the default DB to create the test DB.
    private const string AdminConnectionString =
        "Host=localhost;Port=5433;Database=contactmanager;Username=contactmanager;Password=contactmanager_dev_pwd";

    public const string TestConnectionString =
        "Host=localhost;Port=5433;Database=contactmanager_test;Username=contactmanager;Password=contactmanager_dev_pwd";

    public bool Available { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await using (var admin = new NpgsqlConnection(AdminConnectionString))
            {
                await admin.OpenAsync();
                await using var check = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = 'contactmanager_test'", admin);
                var exists = await check.ExecuteScalarAsync() is not null;
                if (!exists)
                {
                    await using var create = new NpgsqlCommand(
                        "CREATE DATABASE contactmanager_test", admin);
                    await create.ExecuteNonQueryAsync();
                }
            }

            await using var conn = new NpgsqlConnection(TestConnectionString);
            await conn.OpenAsync();
            await using var schema = new NpgsqlCommand(Schema, conn);
            await schema.ExecuteNonQueryAsync();

            Available = true;
        }
        catch (NpgsqlException)
        {
            Available = false;
        }
        catch (SocketException)
        {
            Available = false;
        }
    }

    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(TestConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "TRUNCATE contacts, users RESTART IDENTITY CASCADE", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Mirrors db/init/01_schema.sql so the integration DB matches production.
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS users (
            id            UUID PRIMARY KEY,
            username      VARCHAR(100) NOT NULL UNIQUE,
            password_hash VARCHAR(255) NOT NULL,
            created_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
        );
        CREATE TABLE IF NOT EXISTS contacts (
            id         UUID PRIMARY KEY,
            user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            name       VARCHAR(200) NOT NULL,
            email      VARCHAR(200) NOT NULL,
            phone      VARCHAR(50),
            created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
            updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
        );
        CREATE INDEX IF NOT EXISTS ix_contacts_user_id ON contacts(user_id);
        """;
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresTestFixture>;
