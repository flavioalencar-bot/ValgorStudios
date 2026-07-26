using Valgor.Domain.Users;

namespace Valgor.Application.Common.Interfaces;

public interface IUserStore
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
