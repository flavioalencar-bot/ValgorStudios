using Valgor.Domain.Common;

namespace Valgor.Domain.Users;

public sealed class User : AggregateRoot
{
    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static User Create(string email, string displayName, string passwordHash, UserRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.Raise(new UserCreatedDomainEvent(user.Id, user.Email));
        return user;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        MarkUpdated(utcNow);
    }
}
