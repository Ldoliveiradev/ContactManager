using ContactManager.Application.Common;
using ContactManager.Infrastructure.Identity.Models;
using FluentValidation;

namespace ContactManager.Infrastructure.Identity.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ErrorMessages.Auth.UsernameRequired);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorMessages.Auth.PasswordRequired)
            .MinimumLength(ErrorMessages.Auth.MinPasswordLength).WithMessage(ErrorMessages.Auth.PasswordTooShort);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage(ErrorMessages.Auth.UsernameRequired);
        RuleFor(x => x.Password).NotEmpty().WithMessage(ErrorMessages.Auth.PasswordRequired);
    }
}
