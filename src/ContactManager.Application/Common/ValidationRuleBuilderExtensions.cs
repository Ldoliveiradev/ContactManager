using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FluentValidation;

namespace ContactManager.Application.Common;

public static partial class ValidationRuleBuilderExtensions
{
    [GeneratedRegex(@"^\d{10}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UsPhoneRegex();

    public static IRuleBuilderOptions<T, string> ApplyLengthRule<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minLength,
        int maxLength,
        string requiredMessage,
        string tooShortMessage,
        string tooLongMessage)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(requiredMessage)
            .MinimumLength(minLength).WithMessage(tooShortMessage)
            .MaximumLength(maxLength).WithMessage(tooLongMessage);
    }

    public static IRuleBuilderOptions<T, string> ApplyEmailRule<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int maxLength,
        string requiredMessage,
        string invalidMessage,
        string tooLongMessage)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(requiredMessage)
            .EmailAddress().WithMessage(invalidMessage)
            .MaximumLength(maxLength).WithMessage(tooLongMessage);
    }

    public static IRuleBuilderOptions<T, string> ApplyPasswordRule<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minLength,
        int maxLength,
        string requiredMessage,
        string tooShortMessage,
        string tooLongMessage,
        string weakMessage)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(requiredMessage)
            .MinimumLength(minLength).WithMessage(tooShortMessage)
            .MaximumLength(maxLength).WithMessage(tooLongMessage)
            .Matches(@"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage(weakMessage);
    }

    public static void ApplyAccountNameAndEmailRules<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, string>> firstName,
        Expression<Func<T, string>> lastName,
        Expression<Func<T, string>> email)
    {
        validator.RuleFor(firstName)
            .ApplyLengthRule(
                ErrorMessages.Account.MinNameLength,
                ErrorMessages.Account.MaxNameLength,
                ErrorMessages.Account.FirstNameRequired,
                ErrorMessages.Account.FirstNameTooShort,
                ErrorMessages.Account.FirstNameTooLong);

        validator.RuleFor(lastName)
            .ApplyLengthRule(
                ErrorMessages.Account.MinNameLength,
                ErrorMessages.Account.MaxNameLength,
                ErrorMessages.Account.LastNameRequired,
                ErrorMessages.Account.LastNameTooShort,
                ErrorMessages.Account.LastNameTooLong);

        validator.RuleFor(email)
            .ApplyEmailRule(
                ErrorMessages.Account.MaxEmailLength,
                ErrorMessages.Account.EmailRequired,
                ErrorMessages.Account.EmailInvalid,
                ErrorMessages.Account.EmailTooLong);
    }

    public static IRuleBuilderOptions<T, string?> ApplyOptionalUsPhoneRule<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string invalidMessage)
    {
        return ruleBuilder
            .Must(phone => string.IsNullOrWhiteSpace(phone) || UsPhoneRegex().IsMatch(phone))
            .WithMessage(invalidMessage);
    }
}
