using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace ContactManager.API.Tests;

[Collection("api")]
public class ContactsEndpointsTests
{
    private readonly ApiTestFactory _factory;

    public ContactsEndpointsTests(ApiTestFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthenticatedClientAsync(string username)
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
        return client;
    }

    [Fact]
    public async Task GetContacts_WithoutToken_Returns401()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.GetAsync("/api/v1/contacts");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateThenGet_ReturnsContact()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("crud-user");

        // Act
        var create = await client.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "ada@example.com", phone = "2025550100" });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        var get = await client.GetAsync($"/api/v1/contacts/{created!.Data!.Id}");

        // Assert
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await get.Content.ReadFromJsonAsync<ContactDto>();
        fetched!.Data!.Name.Should().Be("Ada");
        fetched.Data.Email.Should().Be("ada@example.com");
    }

    [Fact]
    public async Task Create_WithInvalidEmail_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("val-user");

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "not-an-email", phone = (string?)null });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetList_ReturnsOnlyOwnContacts()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("alice");
        var bob = await AuthenticatedClientAsync("bob");
        await alice.PostAsJsonAsync("/api/v1/contacts", new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        await bob.PostAsJsonAsync("/api/v1/contacts", new { name = "Grace", email = "grace@example.com", phone = (string?)null });

        // Act
        var aliceList = await alice.GetFromJsonAsync<ContactListDto>("/api/v1/contacts");

        // Assert
        aliceList!.Data.Should().ContainSingle();
        aliceList.Data![0].Name.Should().Be("Ada");
    }

    [Fact]
    public async Task GetList_IgnoresSpoofedOwnerQuery_ReturnsOnlyCallersContacts()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("query-alice");
        var bob = await AuthenticatedClientAsync("query-bob");
        await alice.PostAsJsonAsync("/api/v1/contacts", new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        await bob.PostAsJsonAsync("/api/v1/contacts", new { name = "Grace", email = "grace@example.com", phone = (string?)null });

        // Act
        var resp = await alice.GetFromJsonAsync<ContactListDto>($"/api/v1/contacts?ownerId={Guid.NewGuid()}&userId={Guid.NewGuid()}");

        // Assert
        resp!.Data.Should().ContainSingle();
        resp.Data![0].Name.Should().Be("Ada");
    }

    [Fact]
    public async Task GetOthersContact_Returns403_NotLeakingOwnership()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("alice2");
        var bob = await AuthenticatedClientAsync("bob2");
        var create = await alice.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var aliceContact = await create.Content.ReadFromJsonAsync<ContactDto>();

        // Act
        var resp = await bob.GetAsync($"/api/v1/contacts/{aliceContact!.Data!.Id}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateThenDelete_Works()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("ud-user");
        var create = await client.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        // Act & Assert
        var update = await client.PutAsJsonAsync($"/api/v1/contacts/{created!.Data!.Id}",
            new { name = "Ada L.", email = "ada.l@example.com", phone = "2025550199" });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<ContactDto>())!.Data!.Name.Should().Be("Ada L.");

        // Act & Assert
        var delete = await client.DeleteAsync($"/api/v1/contacts/{created.Data!.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await client.GetAsync($"/api/v1/contacts/{created.Data!.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOthersContact_Returns403_AndDoesNotModify()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("up-alice");
        var bob = await AuthenticatedClientAsync("up-bob");
        var created = await (await alice.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        // Act
        var resp = await bob.PutAsJsonAsync($"/api/v1/contacts/{created!.Data!.Id}",
            new { name = "Hacked", email = "hacked@example.com", phone = (string?)null });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var unchanged = await alice.GetFromJsonAsync<ContactDto>($"/api/v1/contacts/{created.Data!.Id}");
        unchanged!.Data!.Name.Should().Be("Ada");
    }

    [Fact]
    public async Task DeleteOthersContact_Returns403_AndContactSurvives()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("del-alice");
        var bob = await AuthenticatedClientAsync("del-bob");
        var created = await (await alice.PostAsJsonAsync("/api/v1/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        // Act
        var resp = await bob.DeleteAsync($"/api/v1/contacts/{created!.Data!.Id}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var stillThere = await alice.GetAsync($"/api/v1/contacts/{created.Data!.Id}");
        stillThere.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_IgnoresSpoofedOwnerInBody_AssignsCallerAsOwner()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var alice = await AuthenticatedClientAsync("spoof-alice");
        var bob = await AuthenticatedClientAsync("spoof-bob");

        // Act
        var created = await (await bob.PostAsJsonAsync("/api/v1/contacts", new
        {
            name = "Sneaky",
            email = "sneaky@example.com",
            phone = (string?)null,
            userId = Guid.NewGuid(),
            ownerId = Guid.NewGuid(),
        })).Content.ReadFromJsonAsync<ContactDto>();

        // Assert
        (await bob.GetAsync($"/api/v1/contacts/{created!.Data!.Id}")).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var aliceList = await alice.GetFromJsonAsync<ContactListDto>("/api/v1/contacts");
        aliceList!.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData("GET", "/api/v1/contacts")]
    [InlineData("POST", "/api/v1/contacts")]
    [InlineData("GET", "/api/v1/contacts/11111111-1111-1111-1111-111111111111")]
    [InlineData("PUT", "/api/v1/contacts/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/api/v1/contacts/11111111-1111-1111-1111-111111111111")]
    public async Task ContactEndpoints_WithoutToken_Return401(string method, string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(
                new { name = "X", email = "x@example.com", phone = (string?)null });
        }

        // Act
        var resp = await client.SendAsync(request);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record TokenDataDto(string Token);
    private sealed record TokenResponse(TokenDataDto? Data, bool IsSuccess, string? Error);
    private sealed record ContactDataDto(Guid Id, string Name, string Email, string? Phone);
    private sealed record ContactDto(ContactDataDto? Data, bool IsSuccess, string? Error);
    private sealed record ContactListDto(List<ContactDataDto>? Data, int TotalCount, int Page, int PageSize, bool IsSuccess, string? Error);
}
