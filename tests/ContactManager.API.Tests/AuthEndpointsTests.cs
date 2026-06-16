using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace ContactManager.API.Tests;

[Collection("api")]
public class AuthEndpointsTests
{
    private readonly ApiTestFactory _factory;

    public AuthEndpointsTests(ApiTestFactory factory) => _factory = factory;

    [SkippableFact]
    public async Task Register_WithValidData_Returns201()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { username = "newuser", password = "Secret123!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [SkippableFact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var body = new { username = "dupe", password = "Secret123!" };
        await client.PostAsJsonAsync("/api/auth/register", body);
        var resp = await client.PostAsJsonAsync("/api/auth/register", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [SkippableFact]
    public async Task Register_ShortPassword_Returns400()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { username = "shorty", password = "short" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register",
            new { username = "loginuser", password = "Secret123!" });

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { username = "loginuser", password = "Secret123!" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [SkippableFact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register",
            new { username = "user2", password = "Secret123!" });

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { username = "user2", password = "WrongPassword!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LoginResponse(string Token);
}
