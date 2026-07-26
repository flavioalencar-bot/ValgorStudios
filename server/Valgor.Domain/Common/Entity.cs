namespace Valgor.Domain.Common;

/// <summary>
/// Base type for domain entities. Business rules will be added in future iterations.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
