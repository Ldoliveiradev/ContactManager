using ContactManager.Application.Accounts.Interfaces;
using ContactManager.Application.Accounts.Models.Dto;
using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Models.Responses;
using ContactManager.Application.Accounts.Validators;
using ContactManager.Application.Common;
using ContactManager.Domain.Interfaces;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Accounts.Services;

public sealed class AccountService(
    IAccountRepository accounts,
    IPasswordHasher hasher) : IAccountService
{
    private static readonly UpdateAccountRequestValidator UpdateProfileValidator = new();
    private static readonly UpdatePasswordRequestValidator UpdatePasswordValidator = new();

    public async Task<AccountResponse> GetByIdAsync(
        Guid accountId, GetAccountRequest request, CancellationToken ct = default)
    {
        var account = await accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null || account.Id != accountId)
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

    public async Task<AccountResponse> UpdatePasswordAsync(
        Guid accountId, UpdatePasswordRequest request, CancellationToken ct = default)
    {
        var validation = UpdatePasswordValidator.Validate(request);
        if (!validation.IsValid)
            return AccountResponse.Failure(validation.Errors[0].ErrorMessage);

        var account = await accounts.GetByIdAsync(accountId, ct);
        if (account is null)
            return AccountResponse.Failure(ErrorMessages.Account.NotFound);

        if (!hasher.Verify(request.CurrentPassword, account.PasswordHash))
            return AccountResponse.Failure(ErrorMessages.Account.InvalidCurrentPassword);

        account.UpdatePasswordHash(hasher.Hash(request.NewPassword));
        await accounts.UpdateAsync(account, ct);
        return AccountResponse.Success(ToDto(account));
    }

    private static AccountDto ToDto(AccountDomain a) =>
        new(a.Id, a.Username.Value, a.FullName.FirstName, a.FullName.LastName, a.Email.Value);
}
