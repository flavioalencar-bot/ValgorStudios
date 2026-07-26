using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Valgor.Application.Common.Interfaces;
using Valgor.Domain.Users;

namespace Valgor.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, bool applyMigrations, bool seedData, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ValgorDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Valgor.Database");

        if (applyMigrations)
        {
            logger.LogInformation("Applying EF Core migrations");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!seedData)
        {
            return;
        }

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("Seeding initial admin user");

        var admin = User.Create(
            email: "admin@valgor.local",
            displayName: "Valgor Admin",
            passwordHash: passwordHasher.Hash("Valgor@Admin1"),
            role: UserRole.Admin);

        await dbContext.Users.AddAsync(admin, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
