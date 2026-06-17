using ContactManager.Application.Accounts.Models.Dto;
using ContactManager.Application.Common;

namespace ContactManager.Application.Accounts.Models.Responses;

public sealed class AccountResponse : BaseResponse<AccountDto>
{
    public static AccountResponse Success(AccountDto data) =>
        new() { Data = data, IsSuccess = true };

    public static AccountResponse Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
