using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using ContactManager.Application.Common;
using FluentValidation;

namespace ContactManager.Application.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .ApplyLengthRule(
                ErrorMessages.Auth.MinUsernameLength,
                ErrorMessages.Auth.MaxUsernameLength,
                ErrorMessages.Auth.UsernameRequired,
                ErrorMessages.Auth.UsernameTooShort,
                ErrorMessages.Auth.UsernameTooLong);

        this.ApplyAccountNameAndEmailRules(x => x.FirstName, x => x.LastName, x => x.Email);

        RuleFor(x => x.Password)
            .ApplyPasswordRule(
                ErrorMessages.Auth.MinPasswordLength,
                ErrorMessages.Auth.MaxPasswordLength,
                ErrorMessages.Auth.PasswordRequired,
                ErrorMessages.Auth.PasswordTooShort,
                ErrorMessages.Auth.PasswordTooLong,
                ErrorMessages.Auth.PasswordWeak);
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

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ErrorMessages.Auth.UserNotFound);

        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(ErrorMessages.Auth.PasswordRequired);

        RuleFor(x => x.NewPassword)
            .ApplyPasswordRule(
                ErrorMessages.Auth.MinPasswordLength,
                ErrorMessages.Auth.MaxPasswordLength,
                ErrorMessages.Auth.PasswordRequired,
                ErrorMessages.Auth.PasswordTooShort,
                ErrorMessages.Auth.PasswordTooLong,
                ErrorMessages.Auth.PasswordWeak);

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage(ErrorMessages.Auth.PasswordsDoNotMatch);
    }
}
