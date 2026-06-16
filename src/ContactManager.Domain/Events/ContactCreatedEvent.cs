namespace ContactManager.Domain.Events;

public sealed record ContactCreatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid ContactId,
    Guid AccountId,
    string Name,
    string Email) : IDomainEvent;
