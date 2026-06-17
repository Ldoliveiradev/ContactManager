using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IAccountRepository : IBaseRepository<AccountDomain>
{
    // Business accounts are looked up by their (shared) id; the base interface
    // provides GetById/Add/Update. Username lookup is an auth concern on IUserRepository.
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
