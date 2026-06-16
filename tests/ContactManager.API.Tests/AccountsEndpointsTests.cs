using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace ContactManager.API.Tests;

[Collection("api")]
public class AccountsEndpointsTests
{
    private readonly ApiTestFactory _factory;

    public AccountsEndpointsTests(ApiTestFactory factory) => _factory = factory;

    private async Task<(HttpClient client, string username)> AuthenticatedClientAsync(string username)
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            firstName = "Test",
            lastName = "User",
            email = $"{username}@example.com",
            password = "Secret123!"
        });
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { username, password = "Secret123!" });
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.Data!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, username);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/api/accounts");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_ReturnsAuthenticatedAccount()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, username) = await AuthenticatedClientAsync("profile-user");

        var resp = await client.GetAsync("/api/accounts");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = (await resp.Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        account.Username.Should().Be(username);
        account.FirstName.Should().Be("Test");
        account.LastName.Should().Be("User");
        account.Email.Should().Be($"{username}@example.com");
    }

    [Fact]
    public async Task UpdateProfile_PersistsNewValues()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("update-user");

        var update = await client.PutAsJsonAsync("/api/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "grace@example.com" });

        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        updated.FirstName.Should().Be("Grace");
        updated.LastName.Should().Be("Hopper");
        updated.Email.Should().Be("grace@example.com");

        // Re-fetch to confirm the change was persisted, not just echoed back.
        var fetched = (await (await client.GetAsync("/api/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        fetched.FirstName.Should().Be("Grace");
        fetched.Email.Should().Be("grace@example.com");
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidEmail_Returns400()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("bademail-user");

        var resp = await client.PutAsJsonAsync("/api/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "not-an-email" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithoutToken_Returns401()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.PutAsJsonAsync("/api/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "grace@example.com" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePassword_WithCorrectCurrent_Returns204_AndNewPasswordWorks()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, username) = await AuthenticatedClientAsync("pwd-user");

        var change = await client.PutAsJsonAsync("/api/accounts/password",
            new { currentPassword = "Secret123!", newPassword = "NewSecret456!" });

        change.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Old password no longer works.
        var oldLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { username, password = "Secret123!" });
        oldLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // New password works.
        var newLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { username, password = "NewSecret456!" });
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePassword_WithWrongCurrent_Returns400()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("pwd-wrong-user");

        var resp = await client.PutAsJsonAsync("/api/accounts/password",
            new { currentPassword = "WrongPassword!", newPassword = "NewSecret456!" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePassword_WithShortNewPassword_Returns400()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("pwd-short-user");

        var resp = await client.PutAsJsonAsync("/api/accounts/password",
            new { currentPassword = "Secret123!", newPassword = "short" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePassword_WithoutToken_Returns401()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.PutAsJsonAsync("/api/accounts/password",
            new { currentPassword = "Secret123!", newPassword = "NewSecret456!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_OnlyAffectsCallersAccount()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (alice, _) = await AuthenticatedClientAsync("iso-alice");
        var (bob, bobName) = await AuthenticatedClientAsync("iso-bob");

        await alice.PutAsJsonAsync("/api/accounts",
            new { firstName = "Changed", lastName = "ByAlice", email = "alice-new@example.com" });

        // Bob's profile is untouched.
        var bobAccount = (await (await bob.GetAsync("/api/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        bobAccount.Username.Should().Be(bobName);
        bobAccount.FirstName.Should().Be("Test");
        bobAccount.Email.Should().Be($"{bobName}@example.com");
    }

    private sealed record TokenDataDto(string Token);
    private sealed record TokenResponse(TokenDataDto? Data, bool IsSuccess, string? Error);
    private sealed record AccountDataDto(Guid Id, string Username, string FirstName, string LastName, string Email);
    private sealed record AccountResponse(AccountDataDto? Data, bool IsSuccess, string? Error);
}
