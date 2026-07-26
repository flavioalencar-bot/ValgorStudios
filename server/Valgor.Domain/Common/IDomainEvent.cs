namespace Valgor.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOccurredOn OccurredOn { get; }
}

/// <summary>
/// Timestamp wrapper keeps domain events explicit about UTC occurrence.
/// </summary>
public readonly record struct DateTimeOccurredOn(DateTime Utc)
{
    public static DateTimeOccurredOn Now() => new(DateTime.UtcNow);
}
