namespace ContactManager.Domain.Events;

public sealed record ContactDeletedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid ContactId,
    Guid AccountId) : IDomainEvent;
