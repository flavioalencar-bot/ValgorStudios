using Microsoft.EntityFrameworkCore;
using Valgor.Application.Common.Interfaces;
using Valgor.Domain.Users;

namespace Valgor.Infrastructure.Persistence;

public sealed class UserStore(ValgorDbContext dbContext) : IUserStore
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
