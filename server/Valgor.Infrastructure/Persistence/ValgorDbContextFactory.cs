using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Valgor.Infrastructure.Persistence;

public sealed class ValgorDbContextFactory : IDesignTimeDbContextFactory<ValgorDbContext>
{
    public ValgorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Valgor.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5437;Database=valgor;Username=valgor;Password=valgor";

        var options = new DbContextOptionsBuilder<ValgorDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ValgorDbContext(options);
    }
}
