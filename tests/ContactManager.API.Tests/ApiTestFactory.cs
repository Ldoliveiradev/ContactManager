using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ContactManager.API.Tests;

/// <summary>
/// Boots the real API in-memory via WebApplicationFactory, pointed at a dedicated
/// contactmanager_test database so endpoint tests exercise the full stack
/// (controller → service → Npgsql repository → Postgres). Skips gracefully when
/// the database is not reachable.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string AdminConnectionString =
        "Host=localhost;Port=5433;Database=contactmanager;Username=contactmanager;Password=contactmanager_dev_pwd";

    public const string TestConnectionString =
        "Host=localhost;Port=5433;Database=contactmanager_test;Username=contactmanager;Password=contactmanager_dev_pwd";

    public bool DbAvailable { get; private set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = TestConnectionString,
                ["Jwt:Secret"] = "integration-test-signing-secret-at-least-32-characters",
                ["Jwt:Issuer"] = "contactmanager-tests",
                ["Jwt:Audience"] = "contactmanager-test-clients",
                ["Jwt:ExpiryMinutes"] = "60",
            });
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using (var admin = new NpgsqlConnection(AdminConnectionString))
            {
                await admin.OpenAsync();
                await using var check = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = 'contactmanager_test'", admin);
                if (await check.ExecuteScalarAsync() is null)
                {
                    await using var create = new NpgsqlCommand("CREATE DATABASE contactmanager_test", admin);
                    await create.ExecuteNonQueryAsync();
                }
            }

            await using var conn = new NpgsqlConnection(TestConnectionString);
            await conn.OpenAsync();
            await using var schema = new NpgsqlCommand(Schema, conn);
            await schema.ExecuteNonQueryAsync();

            DbAvailable = true;
        }
        catch (NpgsqlException) { DbAvailable = false; }
        catch (SocketException) { DbAvailable = false; }
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(TestConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("TRUNCATE contacts, users RESTART IDENTITY CASCADE", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

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

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiTestFactory>;
