using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Valgor.Infrastructure.Persistence;

namespace Valgor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5437;Database=valgor;Username=valgor;Password=valgor";

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? "localhost:6383";

        services.AddDbContext<ValgorDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "valgor:";
        });

        return services;
    }
}
