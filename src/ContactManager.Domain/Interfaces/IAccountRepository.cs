using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
}
