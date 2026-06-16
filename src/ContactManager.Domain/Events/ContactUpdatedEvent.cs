namespace ContactManager.Domain.Events;

public sealed record ContactUpdatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid ContactId,
    Guid AccountId,
    string Name,
    string Email) : IDomainEvent;
