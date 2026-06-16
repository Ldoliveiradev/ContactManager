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
        var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [SkippableFact]
    public async Task GetContacts_WithoutToken_Returns401()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/api/contacts");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task CreateThenGet_ReturnsContact()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("crud-user");

        var create = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = "+1-202-555-0100" });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        var get = await client.GetAsync($"/api/contacts/{created!.Data!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await get.Content.ReadFromJsonAsync<ContactDto>();
        fetched!.Data!.Name.Should().Be("Ada");
        fetched.Data.Email.Should().Be("ada@example.com");
    }

    [SkippableFact]
    public async Task Create_WithInvalidEmail_Returns400()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("val-user");

        var resp = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "not-an-email", phone = (string?)null });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task GetList_ReturnsOnlyOwnContacts()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("alice");
        var bob = await AuthenticatedClientAsync("bob");

        await alice.PostAsJsonAsync("/api/contacts", new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        await bob.PostAsJsonAsync("/api/contacts", new { name = "Grace", email = "grace@example.com", phone = (string?)null });

        var aliceList = await alice.GetFromJsonAsync<ContactListDto>("/api/contacts");

        aliceList!.Data!.Data.Should().ContainSingle();
        aliceList.Data.Data[0].Name.Should().Be("Ada");
    }

    [SkippableFact]
    public async Task GetOthersContact_Returns404_NotLeakingOwnership()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("alice2");
        var bob = await AuthenticatedClientAsync("bob2");

        var create = await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var aliceContact = await create.Content.ReadFromJsonAsync<ContactDto>();

        var resp = await bob.GetAsync($"/api/contacts/{aliceContact!.Data!.Id}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task UpdateThenDelete_Works()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("ud-user");

        var create = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        var update = await client.PutAsJsonAsync($"/api/contacts/{created!.Data!.Id}",
            new { name = "Ada L.", email = "ada.l@example.com", phone = "+1-202-555-0199" });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<ContactDto>())!.Data!.Name.Should().Be("Ada L.");

        var delete = await client.DeleteAsync($"/api/contacts/{created.Data!.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await client.GetAsync($"/api/contacts/{created.Data!.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task UpdateOthersContact_Returns404_AndDoesNotModify()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("up-alice");
        var bob = await AuthenticatedClientAsync("up-bob");

        var created = await (await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        var resp = await bob.PutAsJsonAsync($"/api/contacts/{created!.Data!.Id}",
            new { name = "Hacked", email = "hacked@example.com", phone = (string?)null });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var unchanged = await alice.GetFromJsonAsync<ContactDto>($"/api/contacts/{created.Data!.Id}");
        unchanged!.Data!.Name.Should().Be("Ada");
    }

    [SkippableFact]
    public async Task DeleteOthersContact_Returns404_AndContactSurvives()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("del-alice");
        var bob = await AuthenticatedClientAsync("del-bob");

        var created = await (await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        var resp = await bob.DeleteAsync($"/api/contacts/{created!.Data!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var stillThere = await alice.GetAsync($"/api/contacts/{created.Data!.Id}");
        stillThere.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Create_IgnoresSpoofedOwnerInBody_AssignsCallerAsOwner()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await ApiTestFactory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("spoof-alice");
        var bob = await AuthenticatedClientAsync("spoof-bob");

        var created = await (await bob.PostAsJsonAsync("/api/contacts", new
        {
            name = "Sneaky",
            email = "sneaky@example.com",
            phone = (string?)null,
            userId = Guid.NewGuid(),
            ownerId = Guid.NewGuid(),
        })).Content.ReadFromJsonAsync<ContactDto>();

        (await bob.GetAsync($"/api/contacts/{created!.Data!.Id}")).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var aliceList = await alice.GetFromJsonAsync<ContactListDto>("/api/contacts");
        aliceList!.Data!.Data.Should().BeEmpty();
    }

    [SkippableTheory]
    [InlineData("GET", "/api/contacts")]
    [InlineData("POST", "/api/contacts")]
    [InlineData("GET", "/api/contacts/11111111-1111-1111-1111-111111111111")]
    [InlineData("PUT", "/api/contacts/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/api/contacts/11111111-1111-1111-1111-111111111111")]
    public async Task ContactEndpoints_WithoutToken_Return401(string method, string url)
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(
                new { name = "X", email = "x@example.com", phone = (string?)null });
        }

        var resp = await client.SendAsync(request);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record TokenResponse(string Token);
    private sealed record ContactDataDto(Guid Id, string Name, string Email, string? Phone);
    private sealed record ContactDto(ContactDataDto? Data, bool IsSuccess, string? Error);
    private sealed record ContactListInnerDto(List<ContactDataDto> Data, bool IsSuccess, string? Error);
    private sealed record ContactListDto(ContactListInnerDto? Data, int TotalCount, int Page, int PageSize, bool IsSuccess, string? Error);
}
