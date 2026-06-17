using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Common;
using FluentValidation;

namespace ContactManager.Application.Contacts.Validators;

public sealed class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .ApplyLengthRule(
                ErrorMessages.Contact.MinNameLength,
                ErrorMessages.Contact.MaxNameLength,
                ErrorMessages.Contact.NameRequired,
                ErrorMessages.Contact.NameTooShort,
                ErrorMessages.Contact.NameTooLong);

        RuleFor(x => x.Email)
            .ApplyEmailRule(
                ErrorMessages.Contact.MaxEmailLength,
                ErrorMessages.Contact.EmailRequired,
                ErrorMessages.Contact.EmailInvalid,
                ErrorMessages.Contact.EmailTooLong);

        RuleFor(x => x.Phone)
            .ApplyOptionalUsPhoneRule(ErrorMessages.Contact.PhoneInvalid);
    }
}

public sealed class UpdateContactRequestValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .ApplyLengthRule(
                ErrorMessages.Contact.MinNameLength,
                ErrorMessages.Contact.MaxNameLength,
                ErrorMessages.Contact.NameRequired,
                ErrorMessages.Contact.NameTooShort,
                ErrorMessages.Contact.NameTooLong);

        RuleFor(x => x.Email)
            .ApplyEmailRule(
                ErrorMessages.Contact.MaxEmailLength,
                ErrorMessages.Contact.EmailRequired,
                ErrorMessages.Contact.EmailInvalid,
                ErrorMessages.Contact.EmailTooLong);

        RuleFor(x => x.Phone)
            .ApplyOptionalUsPhoneRule(ErrorMessages.Contact.PhoneInvalid);
    }
}
