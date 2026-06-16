using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IContactRepository
{
    Task<(IReadOnlyList<ContactDomain> Items, int TotalCount)> GetByUserAsync(
        Guid userId, string? search, string? sortBy, bool sortDesc, int page, int pageSize, CancellationToken ct = default);
    Task<ContactDomain?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ContactDomain contact, CancellationToken ct = default);
    Task UpdateAsync(ContactDomain contact, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
