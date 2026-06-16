using ContactManager.Domain.Models;

namespace ContactManager.Domain.Interfaces;

public interface IContactRepository
{
    Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetByAccountAsync(
        Guid accountId, string? search, string? sortBy, bool sortDesc, int page, int pageSize,
        CancellationToken ct = default);

    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Contact contact, CancellationToken ct = default);
    Task UpdateAsync(Contact contact, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
