using ContactManager.Domain.Entities;

namespace ContactManager.Domain.Repositories;

public interface IContactRepository
{
    Task<(IReadOnlyList<Contact> Items, int TotalCount)> GetByUserAsync(
        Guid userId, string? search, string? sortBy, bool sortDesc, int page, int pageSize, CancellationToken ct = default);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Contact contact, CancellationToken ct = default);
    Task UpdateAsync(Contact contact, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
