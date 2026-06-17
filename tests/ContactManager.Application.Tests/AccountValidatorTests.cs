using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Validators;
using FluentAssertions;

namespace ContactManager.Application.Tests;

public class AccountValidatorTests
{
    private readonly UpdateAccountRequestValidator _profileValidator = new();

    [Fact]
    public void ProfileValidator_ValidRequest_Passes()
    {
        // Act
        var result = _profileValidator.Validate(
            new UpdateAccountRequest("Grace", "Hopper", "grace@example.com"));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Hopper", "grace@example.com")]
    [InlineData("  ", "Hopper", "grace@example.com")]
    public void ProfileValidator_EmptyFirstName_Fails(string first, string last, string email)
    {
        // Act
        var result = _profileValidator.Validate(new UpdateAccountRequest(first, last, email));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.FirstName));
    }

    [Theory]
    [InlineData("Grace", "", "grace@example.com")]
    [InlineData("Grace", "  ", "grace@example.com")]
    public void ProfileValidator_EmptyLastName_Fails(string first, string last, string email)
    {
        // Act
        var result = _profileValidator.Validate(new UpdateAccountRequest(first, last, email));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ProfileValidator_InvalidEmail_Fails(string email)
    {
        // Act
        var result = _profileValidator.Validate(new UpdateAccountRequest("Grace", "Hopper", email));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAccountRequest.Email));
    }
}
