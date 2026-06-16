using ContactManager.Application.Auth.Models;
using ContactManager.Infrastructure.Identity.Validators;
using FluentAssertions;

namespace ContactManager.Infrastructure.Tests;

public class AuthValidatorTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    public void RegisterValidator_EmptyUsername_Fails(string username, string password)
    {
        var result = _registerValidator.Validate(
            new RegisterRequest(username, "First", "Last", "test@example.com", password));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Username));
    }

    [Theory]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public void RegisterValidator_InvalidPassword_Fails(string username, string password)
    {
        var result = _registerValidator.Validate(
            new RegisterRequest(username, "First", "Last", "test@example.com", password));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void RegisterValidator_ValidRequest_Passes()
    {
        var result = _registerValidator.Validate(
            new RegisterRequest("demo", "First", "Last", "demo@example.com", "Secret123!"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("demo", "")]
    public void LoginValidator_MissingFields_Fails(string username, string password)
    {
        var result = _loginValidator.Validate(new LoginRequest(username, password));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginValidator_ValidRequest_Passes()
    {
        var result = _loginValidator.Validate(new LoginRequest("demo", "Secret123!"));
        result.IsValid.Should().BeTrue();
    }
}
