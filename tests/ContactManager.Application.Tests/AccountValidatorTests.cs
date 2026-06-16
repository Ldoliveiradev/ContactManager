using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Validators;
using FluentAssertions;

namespace ContactManager.Application.Tests;

public class AccountValidatorTests
{
    private readonly UpdateAccountRequestValidator _profileValidator = new();
    private readonly UpdatePasswordRequestValidator _passwordValidator = new();

    [Fact]
    public void ProfileValidator_ValidRequest_Passes()
    {
        var result = _profileValidator.Validate(
            new UpdateAccountRequest("Grace", "Hopper", "grace@example.com"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Hopper", "grace@example.com")]
    [InlineData("  ", "Hopper", "grace@example.com")]
    public void ProfileValidator_EmptyFirstName_Fails(string first, string last, string email)
    {
        var result = _profileValidator.Validate(new UpdateAccountRequest(first, last, email));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.FirstName));
    }

    [Theory]
    [InlineData("Grace", "", "grace@example.com")]
    [InlineData("Grace", "  ", "grace@example.com")]
    public void ProfileValidator_EmptyLastName_Fails(string first, string last, string email)
    {
        var result = _profileValidator.Validate(new UpdateAccountRequest(first, last, email));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ProfileValidator_InvalidEmail_Fails(string email)
    {
        var result = _profileValidator.Validate(new UpdateAccountRequest("Grace", "Hopper", email));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.Email));
    }

    [Fact]
    public void PasswordValidator_ValidRequest_Passes()
    {
        var result = _passwordValidator.Validate(
            new UpdatePasswordRequest("OldSecret123!", "NewSecret456!"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PasswordValidator_EmptyCurrentPassword_Fails(string current)
    {
        var result = _passwordValidator.Validate(new UpdatePasswordRequest(current, "NewSecret456!"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePasswordRequest.CurrentPassword));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void PasswordValidator_InvalidNewPassword_Fails(string newPassword)
    {
        var result = _passwordValidator.Validate(new UpdatePasswordRequest("OldSecret123!", newPassword));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePasswordRequest.NewPassword));
    }
}
