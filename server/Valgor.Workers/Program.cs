using Serilog;
using Valgor.Application;
using Valgor.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((_, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<Valgor.Workers.Worker>();

    var host = builder.Build();
    Log.Information("Valgor Workers starting");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Valgor Workers terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
