using Valgor.Domain.Common;

namespace Valgor.Domain.Users;

public sealed class UserCreatedDomainEvent(Guid userId, string email) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
}
