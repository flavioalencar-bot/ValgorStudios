namespace Valgor.Domain.Common;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOccurredOn OccurredOn { get; } = DateTimeOccurredOn.Now();
}
