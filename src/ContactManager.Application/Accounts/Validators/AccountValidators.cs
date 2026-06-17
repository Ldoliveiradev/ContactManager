using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Common;
using FluentValidation;

namespace ContactManager.Application.Accounts.Validators;

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        this.ApplyAccountNameAndEmailRules(x => x.FirstName, x => x.LastName, x => x.Email);
    }
}
