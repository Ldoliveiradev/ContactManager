using ContactManager.Application.Common;
using ContactManager.Infrastructure.Auth.Models;
using FluentValidation;

namespace ContactManager.Infrastructure.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ErrorMessages.Auth.UsernameRequired);

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
