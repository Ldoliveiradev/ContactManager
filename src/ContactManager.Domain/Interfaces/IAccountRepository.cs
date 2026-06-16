using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IAccountRepository : IBaseRepository<AccountDomain>
{
    Task<AccountDomain?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
