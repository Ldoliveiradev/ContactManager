using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Models.Responses;

namespace ContactManager.Application.Accounts.Interfaces;

public interface IAccountService
{
    Task<AccountResponse> GetByIdAsync(Guid accountId, GetAccountRequest request, CancellationToken ct = default);
    Task<AccountResponse> UpdateProfileAsync(Guid accountId, UpdateAccountRequest request, CancellationToken ct = default);
}
