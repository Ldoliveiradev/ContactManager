using ContactManager.Application.Accounts.Interfaces;
using ContactManager.Application.Accounts.Models.Dto;
using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Models.Responses;
using ContactManager.Application.Accounts.Validators;
using ContactManager.Application.Common;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Accounts.Services;

public sealed class AccountService(IAccountRepository accounts) : IAccountService
{
    private static readonly UpdateAccountRequestValidator UpdateProfileValidator = new();

    public async Task<AccountResponse> GetByIdAsync(
        Guid accountId, GetAccountRequest request, CancellationToken ct = default)
    {
        if (request.AccountId != accountId)
            return AccountResponse.Failure(ErrorMessages.Auth.Forbidden);

        var account = await accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AccountResponse.Failure(ErrorMessages.Account.NotFound);

        return AccountResponse.Success(ToDto(account));
    }

    public async Task<AccountResponse> UpdateProfileAsync(
        Guid accountId, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var validation = UpdateProfileValidator.Validate(request);
        if (!validation.IsValid)
            return AccountResponse.Failure(validation.Errors[0].ErrorMessage);

        var account = await accounts.GetByIdAsync(accountId, ct);
        if (account is null)
            return AccountResponse.Failure(ErrorMessages.Account.NotFound);

        account.UpdateProfile(request.FirstName, request.LastName, request.Email);
        await accounts.UpdateAsync(account, ct);
        return AccountResponse.Success(ToDto(account));
    }

    private static AccountDto ToDto(AccountDomain a) =>
        new(a.Id, a.FullName.FirstName, a.FullName.LastName, a.Email.Value);
}
