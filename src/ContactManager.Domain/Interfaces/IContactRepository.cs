using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IContactRepository : IBaseRepository<ContactDomain>
{
    Task<(IReadOnlyList<ContactDomain> Items, int TotalCount)> GetByAccountAsync(
        Guid accountId, string? search, string? sortBy, bool sortDesc, int page, int pageSize,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
