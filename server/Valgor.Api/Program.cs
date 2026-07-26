using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using Valgor.Application;
using Valgor.Contracts.Health;
using Valgor.Infrastructure;
using Valgor.Infrastructure.Persistence;

const string AppVersion = "0.1.0";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Valgor API",
            Version = AppVersion,
            Description = "API oficial do ecossistema Valgor Studios."
        });
    });

    var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
        ?? "Host=localhost;Port=5437;Database=valgor;Username=valgor;Password=valgor";
    var redisConnection = builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6383";

    builder.Services.AddHealthChecks()
        .AddNpgSql(postgresConnection, name: "postgres")
        .AddRedis(redisConnection, name: "redis")
        .AddDbContextCheck<ValgorDbContext>("ef-core");

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Valgor API {AppVersion}");
        });
    }

    app.MapGet("/health", () => Results.Ok(new HealthResponse("ok", AppVersion)))
        .WithName("GetHealth")
        .WithTags("Health")
        .Produces<HealthResponse>(StatusCodes.Status200OK);

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapControllers();

    Log.Information("Valgor API {Version} starting", AppVersion);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Valgor API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
