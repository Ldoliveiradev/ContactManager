using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Common;
using FluentValidation;

namespace ContactManager.Application.Accounts.Validators;

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(ErrorMessages.Account.FirstNameRequired);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(ErrorMessages.Account.LastNameRequired);
        RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorMessages.Account.EmailRequired)
            .EmailAddress().WithMessage(ErrorMessages.Account.EmailInvalid);
    }
}

public sealed class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(ErrorMessages.Auth.PasswordRequired);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ErrorMessages.Auth.PasswordRequired)
            .MinimumLength(ErrorMessages.Auth.MinPasswordLength).WithMessage(ErrorMessages.Auth.PasswordTooShort);
    }
}
