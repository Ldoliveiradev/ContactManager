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

    public async Task InitializeAsync()
    {
        // No try/catch: if the PostgreSQL test database is unreachable, this throws
        // and every repository test fails loudly rather than silently skipping.
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
        // Drop and recreate so schema changes (e.g. splitting users/accounts) always take effect.
        await using var drop = new NpgsqlCommand(DropSchema, conn);
        await drop.ExecuteNonQueryAsync();
        await using var schema = new NpgsqlCommand(Schema, conn);
        await schema.ExecuteNonQueryAsync();
    }

    public static async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(TestConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "TRUNCATE contacts, accounts, users RESTART IDENTITY CASCADE", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Drops all tables in dependency order so re-running InitializeAsync always starts clean.
    private const string DropSchema = """
        DROP TABLE IF EXISTS contacts;
        DROP TABLE IF EXISTS accounts;
        DROP TABLE IF EXISTS users;
        """;

    // Mirrors db/init/01_schema.sql so the integration DB matches production.
    // users:    authentication identity (id, username, password_hash).
    // accounts: business identity (id FK→users.id, first_name, last_name, email).
    // contacts: account manager's book (account_id FK→accounts.id).
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS users (
            id            UUID         PRIMARY KEY,
            username      VARCHAR(100) NOT NULL UNIQUE,
            password_hash VARCHAR(255) NOT NULL,
            created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
            updated_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
        );
        CREATE TABLE IF NOT EXISTS accounts (
            id         UUID         PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
            first_name VARCHAR(100) NOT NULL,
            last_name  VARCHAR(100) NOT NULL,
            email      VARCHAR(200) NOT NULL UNIQUE,
            created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
            updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
        );
        CREATE TABLE IF NOT EXISTS contacts (
            id         UUID         PRIMARY KEY,
            account_id UUID         NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
            name       VARCHAR(200) NOT NULL,
            email      VARCHAR(200) NOT NULL,
            phone      VARCHAR(50),
            created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
            updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
        );
        CREATE INDEX IF NOT EXISTS ix_contacts_account_id ON contacts(account_id);
        """;
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresTestFixture>;
