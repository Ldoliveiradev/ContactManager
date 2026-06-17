using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using ContactManager.Application.Auth.Validators;
using FluentAssertions;

namespace ContactManager.Application.Tests;

public class AuthValidatorTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly ChangePasswordRequestValidator _changePasswordValidator = new();

    // ---- Register ----

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("  ", "Secret123!")]
    public void RegisterValidator_EmptyUsername_Fails(string username, string password)
    {
        // Act
        var result = _registerValidator.Validate(
            new RegisterRequest(username, "First", "Last", "test@example.com", password));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Username));
    }

    [Theory]
    [InlineData("demo", "")]
    [InlineData("demo", "short")]
    public void RegisterValidator_InvalidPassword_Fails(string username, string password)
    {
        // Act
        var result = _registerValidator.Validate(
            new RegisterRequest(username, "First", "Last", "test@example.com", password));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void RegisterValidator_ValidRequest_Passes()
    {
        // Act
        var result = _registerValidator.Validate(
            new RegisterRequest("demo", "First", "Last", "demo@example.com", "Secret123!"));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ---- Login ----

    [Theory]
    [InlineData("", "Secret123!")]
    [InlineData("demo", "")]
    public void LoginValidator_MissingFields_Fails(string username, string password)
    {
        // Act
        var result = _loginValidator.Validate(new LoginRequest(username, password));

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginValidator_ValidRequest_Passes()
    {
        // Act
        var result = _loginValidator.Validate(new LoginRequest("demo", "Secret123!"));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ---- ChangePassword ----

    [Fact]
    public void ChangePasswordValidator_ValidRequest_Passes()
    {
        // Act
        var result = _changePasswordValidator.Validate(
            new ChangePasswordRequest(Guid.NewGuid(), "OldSecret123!", "NewSecret456!", "NewSecret456!"));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePasswordValidator_EmptyCurrentPassword_Fails(string current)
    {
        // Act
        var result = _changePasswordValidator.Validate(
            new ChangePasswordRequest(Guid.NewGuid(), current, "NewSecret456!", "NewSecret456!"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void ChangePasswordValidator_InvalidNewPassword_Fails(string newPassword)
    {
        // Act
        var result = _changePasswordValidator.Validate(
            new ChangePasswordRequest(Guid.NewGuid(), "OldSecret123!", newPassword, newPassword));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    public void ChangePasswordValidator_EmptyUserId_Fails()
    {
        var result = _changePasswordValidator.Validate(
            new ChangePasswordRequest(Guid.Empty, "OldSecret123!", "NewSecret456!", "NewSecret456!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.UserId));
    }
}
