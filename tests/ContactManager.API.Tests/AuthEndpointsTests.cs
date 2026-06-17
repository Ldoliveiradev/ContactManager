using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace ContactManager.API.Tests;

[Collection("api")]
public class AuthEndpointsTests
{
    private readonly ApiTestFactory _factory;

    public AuthEndpointsTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "newuser",
            firstName = "New",
            lastName = "User",
            email = "newuser@example.com",
            password = "Secret123!"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var body = new { username = "dupe", firstName = "Dupe", lastName = "User", email = "dupe@example.com", password = "Secret123!" };
        await client.PostAsJsonAsync("/api/v1/auth/register", body);

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", body);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "emailowner",
            firstName = "Email",
            lastName = "Owner",
            email = "shared@example.com",
            password = "Secret123!"
        });

        // Act — different username, same email.
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "otheruser",
            firstName = "Other",
            lastName = "User",
            email = "shared@example.com",
            password = "Secret123!"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "shorty",
            firstName = "Short",
            lastName = "Pwd",
            email = "shorty@example.com",
            password = "short"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "bademail",
            firstName = "Bad",
            lastName = "Email",
            email = "not-an-email",
            password = "Secret123!"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_BlankFirstName_Returns400(string firstName)
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "blankfirst",
            firstName,
            lastName = "User",
            email = "blankfirst@example.com",
            password = "Secret123!"
        });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "loginuser",
            firstName = "Login",
            lastName = "User",
            email = "loginuser@example.com",
            password = "Secret123!"
        });

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "loginuser", password = "Secret123!" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        payload!.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithEmailInsteadOfUsername_ReturnsToken()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "emaillogin",
            firstName = "Email",
            lastName = "Login",
            email = "emaillogin@example.com",
            password = "Secret123!"
        });

        // Act — sign in with the email in the username field.
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "emaillogin@example.com", password = "Secret123!" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        payload!.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Arrange
        await ApiTestFactory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            username = "user2",
            firstName = "User",
            lastName = "Two",
            email = "user2@example.com",
            password = "Secret123!"
        });

        // Act
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "user2", password = "WrongPassword!" });

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LoginDataDto(string Token);
    private sealed record LoginResponse(LoginDataDto? Data, bool IsSuccess, string? Error);
}
