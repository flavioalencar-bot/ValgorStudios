using Microsoft.EntityFrameworkCore;

namespace Valgor.Infrastructure.Persistence;

public sealed class ValgorDbContext(DbContextOptions<ValgorDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ValgorDbContext).Assembly);
    }
}
