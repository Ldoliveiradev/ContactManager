using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IAccountRepository : IBaseRepository<AccountDomain>
{
    // Business accounts are looked up by their (shared) id; the base interface
    // provides GetById/Add/Update. Username lookup is an auth concern on IUserRepository.
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    // Overload used by profile updates: excludes the caller's own account so an
    // unchanged email is not flagged as a duplicate against itself.
    Task<bool> ExistsByEmailAsync(string email, Guid excludeId, CancellationToken ct = default);
}
