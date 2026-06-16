using ContactManager.Domain.Entities;

namespace ContactManager.Application.Abstractions;

/// <summary>
/// Persistence contract for contacts. Implemented in Infrastructure with hand-written SQL.
/// Reads are scoped to a user where ownership matters.
/// </summary>
public interface IContactRepository
{
    Task<IReadOnlyList<Contact>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Contact contact, CancellationToken ct = default);
    Task UpdateAsync(Contact contact, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
