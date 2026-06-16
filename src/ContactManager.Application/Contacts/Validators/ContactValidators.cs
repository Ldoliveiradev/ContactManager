using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Common;
using FluentValidation;

namespace ContactManager.Application.Contacts.Validators;

public sealed class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(ErrorMessages.Contact.NameRequired);
        RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorMessages.Contact.EmailRequired)
            .EmailAddress().WithMessage(ErrorMessages.Contact.EmailInvalid);
    }
}

public sealed class UpdateContactRequestValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(ErrorMessages.Contact.NameRequired);
        RuleFor(x => x.Email).NotEmpty().WithMessage(ErrorMessages.Contact.EmailRequired)
            .EmailAddress().WithMessage(ErrorMessages.Contact.EmailInvalid);
    }
}
