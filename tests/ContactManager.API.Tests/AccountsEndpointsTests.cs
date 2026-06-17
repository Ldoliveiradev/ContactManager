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
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username,
            firstName = "Test",
            lastName = "User",
            email = $"{username}@example.com",
            password = "Secret123!"
        });
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username, password = "Secret123!" });
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.Data!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, username);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/api/v1/accounts");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_ReturnsAuthenticatedAccount()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, username) = await AuthenticatedClientAsync("profile-user");

        // Act
        var resp = await client.GetAsync("/api/v1/accounts");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = (await resp.Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        account.FirstName.Should().Be("Test");
        account.LastName.Should().Be("User");
        account.Email.Should().Be($"{username}@example.com");
    }

    [Fact]
    public async Task GetProfile_IgnoresSpoofedOwnerQuery_AndReturnsAuthenticatedAccount()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (alice, aliceName) = await AuthenticatedClientAsync("profile-alice");
        var (bob, _) = await AuthenticatedClientAsync("profile-bob");
        var bobAccount = (await (await bob.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;

        // Act
        var resp = await alice.GetAsync($"/api/v1/accounts?id={bobAccount.Id}&userId={bobAccount.Id}&ownerId={bobAccount.Id}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = (await resp.Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        account.Email.Should().Be($"{aliceName}@example.com");
    }

    [Fact]
    public async Task UpdateProfile_PersistsNewValues()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("update-user");

        // Act
        var update = await client.PutAsJsonAsync("/api/v1/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "grace@example.com" });

        // Assert
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        updated.FirstName.Should().Be("Grace");
        updated.LastName.Should().Be("Hopper");
        updated.Email.Should().Be("grace@example.com");

        // Re-fetch to confirm the change was persisted, not just echoed back.
        var fetched = (await (await client.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        fetched.FirstName.Should().Be("Grace");
        fetched.Email.Should().Be("grace@example.com");
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidEmail_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("bademail-user");

        // Act
        var resp = await client.PutAsJsonAsync("/api/v1/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "not-an-email" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WithoutToken_Returns401()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PutAsJsonAsync("/api/v1/accounts",
            new { firstName = "Grace", lastName = "Hopper", email = "grace@example.com" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePassword_WithCorrectCurrent_Returns204_AndNewPasswordWorks()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, username) = await AuthenticatedClientAsync("pwd-user");
        var account = (await (await client.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;

        // Act
        var change = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { userId = account.Id, currentPassword = "Secret123!", newPassword = "NewSecret456!", confirmNewPassword = "NewSecret456!" });

        // Assert
        change.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Old password no longer works.
        var oldLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username, password = "Secret123!" });
        oldLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // New password works.
        var newLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username, password = "NewSecret456!" });
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePassword_WithWrongCurrent_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("pwd-wrong-user");
        var account = (await (await client.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { userId = account.Id, currentPassword = "WrongPassword!", newPassword = "NewSecret456!", confirmNewPassword = "NewSecret456!" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePassword_WithShortNewPassword_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("pwd-short-user");
        var account = (await (await client.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { userId = account.Id, currentPassword = "Secret123!", newPassword = "short", confirmNewPassword = "short" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePassword_WithoutToken_Returns401()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { userId = Guid.NewGuid(), currentPassword = "Secret123!", newPassword = "NewSecret456!", confirmNewPassword = "NewSecret456!" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePassword_WithMismatchedUserId_Returns403()
    {
        await ApiTestFactory.ResetDatabaseAsync();
        var (client, _) = await AuthenticatedClientAsync("pwd-forbidden-user");

        var resp = await client.PostAsJsonAsync("/api/v1/auth/change-password",
            new { userId = Guid.NewGuid(), currentPassword = "Secret123!", newPassword = "NewSecret456!", confirmNewPassword = "NewSecret456!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProfile_OnlyAffectsCallersAccount()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (alice, _) = await AuthenticatedClientAsync("iso-alice");
        var (bob, bobName) = await AuthenticatedClientAsync("iso-bob");

        // Act
        await alice.PutAsJsonAsync("/api/v1/accounts",
            new { firstName = "Changed", lastName = "ByAlice", email = "alice-new@example.com" });

        // Assert
        // Bob's profile is untouched.
        var bobAccount = (await (await bob.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        bobAccount.FirstName.Should().Be("Test");
        bobAccount.Email.Should().Be($"{bobName}@example.com");
    }

    [Fact]
    public async Task UpdateProfile_IgnoresSpoofedOwnerInBody_AndOnlyUpdatesCaller()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var (alice, _) = await AuthenticatedClientAsync("body-alice");
        var (bob, bobName) = await AuthenticatedClientAsync("body-bob");
        var bobAccount = (await (await bob.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;

        // Act
        var resp = await alice.PutAsJsonAsync("/api/v1/accounts", new
        {
            id = bobAccount.Id,
            userId = bobAccount.Id,
            ownerId = bobAccount.Id,
            firstName = "AliceChanged",
            lastName = "OwnedByToken",
            email = "alice-token@example.com"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var aliceAccount = (await (await alice.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        aliceAccount.FirstName.Should().Be("AliceChanged");
        aliceAccount.Email.Should().Be("alice-token@example.com");

        var bobAfter = (await (await bob.GetAsync("/api/v1/accounts"))
            .Content.ReadFromJsonAsync<AccountResponse>())!.Data!;
        bobAfter.FirstName.Should().Be("Test");
        bobAfter.Email.Should().Be($"{bobName}@example.com");
    }

    private sealed record TokenDataDto(string Token);
    private sealed record TokenResponse(TokenDataDto? Data, bool IsSuccess, string? Error);
    private sealed record AccountDataDto(Guid Id, string FirstName, string LastName, string Email);
    private sealed record AccountResponse(AccountDataDto? Data, bool IsSuccess, string? Error);
}
