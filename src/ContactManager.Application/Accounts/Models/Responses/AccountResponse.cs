using ContactManager.Application.Accounts.Models.Dto;

namespace ContactManager.Application.Accounts.Models.Responses;

public sealed class AccountResponse
{
    public AccountDto? Data { get; init; }
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static AccountResponse Success(AccountDto data) =>
        new() { Data = data, IsSuccess = true };

    public static AccountResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
