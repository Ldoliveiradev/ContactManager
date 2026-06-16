using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Validators;
using FluentAssertions;

namespace ContactManager.Application.Tests;

public class ContactValidatorTests
{
    private readonly CreateContactRequestValidator _createValidator = new();
    private readonly UpdateContactRequestValidator _updateValidator = new();

    [Theory]
    [InlineData("", "ada@example.com")]
    [InlineData("  ", "ada@example.com")]
    public void CreateValidator_EmptyName_Fails(string name, string email)
    {
        var result = _createValidator.Validate(new CreateContactRequest(name, email, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContactRequest.Name));
    }

    [Theory]
    [InlineData("Ada", "")]
    [InlineData("Ada", "bad")]
    [InlineData("Ada", "bad@")]
    public void CreateValidator_InvalidEmail_Fails(string name, string email)
    {
        var result = _createValidator.Validate(new CreateContactRequest(name, email, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContactRequest.Email));
    }

    [Fact]
    public void CreateValidator_ValidRequest_Passes()
    {
        var result = _createValidator.Validate(new CreateContactRequest("Ada", "ada@example.com", null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "ada@example.com")]
    [InlineData("Ada", "notanemail")]
    public void UpdateValidator_InvalidFields_Fails(string name, string email)
    {
        var result = _updateValidator.Validate(new UpdateContactRequest(name, email, null));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateValidator_ValidRequest_Passes()
    {
        var result = _updateValidator.Validate(new UpdateContactRequest("Ada", "ada@example.com", null));
        result.IsValid.Should().BeTrue();
    }
}
