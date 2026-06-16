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

    /// <summary>Registers a user, logs in, and returns a client with the bearer token set.</summary>
    private async Task<HttpClient> AuthenticatedClientAsync(string username)
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register",
            new { username, password = "Secret123!" });
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
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/api/contacts");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task CreateThenGet_ReturnsContact()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("crud-user");

        var create = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = "+1-202-555-0100" });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        var get = await client.GetAsync($"/api/contacts/{created!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await get.Content.ReadFromJsonAsync<ContactDto>();
        fetched!.Name.Should().Be("Ada");
        fetched.Email.Should().Be("ada@example.com");
    }

    [SkippableFact]
    public async Task Create_WithInvalidEmail_Returns400()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("val-user");

        var resp = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "not-an-email", phone = (string?)null });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task GetList_ReturnsOnlyOwnContacts()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("alice");
        var bob = await AuthenticatedClientAsync("bob");

        await alice.PostAsJsonAsync("/api/contacts", new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        await bob.PostAsJsonAsync("/api/contacts", new { name = "Grace", email = "grace@example.com", phone = (string?)null });

        var aliceList = await alice.GetFromJsonAsync<List<ContactDto>>("/api/contacts");

        aliceList.Should().ContainSingle();
        aliceList![0].Name.Should().Be("Ada");
    }

    [SkippableFact]
    public async Task GetOthersContact_Returns404_NotLeakingOwnership()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("alice2");
        var bob = await AuthenticatedClientAsync("bob2");

        var create = await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var aliceContact = await create.Content.ReadFromJsonAsync<ContactDto>();

        // Bob tries to read Alice's contact by id → must look not-found, not forbidden.
        var resp = await bob.GetAsync($"/api/contacts/{aliceContact!.Id}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task UpdateThenDelete_Works()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();
        var client = await AuthenticatedClientAsync("ud-user");

        var create = await client.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null });
        var created = await create.Content.ReadFromJsonAsync<ContactDto>();

        var update = await client.PutAsJsonAsync($"/api/contacts/{created!.Id}",
            new { name = "Ada L.", email = "ada.l@example.com", phone = "+1-202-555-0199" });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<ContactDto>())!.Name.Should().Be("Ada L.");

        var delete = await client.DeleteAsync($"/api/contacts/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await client.GetAsync($"/api/contacts/{created.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task UpdateOthersContact_Returns404_AndDoesNotModify()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("up-alice");
        var bob = await AuthenticatedClientAsync("up-bob");

        var created = await (await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        // Bob attempts to edit Alice's contact → 404 (ownership not leaked).
        var resp = await bob.PutAsJsonAsync($"/api/contacts/{created!.Id}",
            new { name = "Hacked", email = "hacked@example.com", phone = (string?)null });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // And Alice's contact is unchanged.
        var unchanged = await alice.GetFromJsonAsync<ContactDto>($"/api/contacts/{created.Id}");
        unchanged!.Name.Should().Be("Ada");
    }

    [SkippableFact]
    public async Task DeleteOthersContact_Returns404_AndContactSurvives()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("del-alice");
        var bob = await AuthenticatedClientAsync("del-bob");

        var created = await (await alice.PostAsJsonAsync("/api/contacts",
            new { name = "Ada", email = "ada@example.com", phone = (string?)null }))
            .Content.ReadFromJsonAsync<ContactDto>();

        var resp = await bob.DeleteAsync($"/api/contacts/{created!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // The contact still exists for its real owner.
        var stillThere = await alice.GetAsync($"/api/contacts/{created.Id}");
        stillThere.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Create_IgnoresSpoofedOwnerInBody_AssignsCallerAsOwner()
    {
        Skip.IfNot(_factory.DbAvailable, "PostgreSQL test database not available.");
        await _factory.ResetDatabaseAsync();

        var alice = await AuthenticatedClientAsync("spoof-alice");
        var bob = await AuthenticatedClientAsync("spoof-bob");

        // Bob tries to create a contact "owned" by Alice by smuggling userId/ownerId fields.
        // The API binds only name/email/phone and takes the owner from the JWT, so these
        // extra fields are ignored — the contact belongs to Bob, and Alice never sees it.
        var created = await (await bob.PostAsJsonAsync("/api/contacts", new
        {
            name = "Sneaky",
            email = "sneaky@example.com",
            phone = (string?)null,
            userId = Guid.NewGuid(),
            ownerId = Guid.NewGuid(),
        })).Content.ReadFromJsonAsync<ContactDto>();

        // Bob can see it; Alice cannot.
        (await bob.GetAsync($"/api/contacts/{created!.Id}")).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var aliceList = await alice.GetFromJsonAsync<List<ContactDto>>("/api/contacts");
        aliceList!.Should().BeEmpty();
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
        var client = _factory.CreateClient(); // no Authorization header

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
    private sealed record ContactDto(Guid Id, string Name, string Email, string? Phone);
}
