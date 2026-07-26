using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using Valgor.Api.Middleware;
using Valgor.Application;
using Valgor.Contracts.Health;
using Valgor.Contracts.Versioning;
using Valgor.Infrastructure;
using Valgor.Infrastructure.Persistence;

const string AppVersion = "0.1.0";
const string ProductName = "Valgor";

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

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando o esquema Bearer.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Admin", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "http://127.0.0.1:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var postgresConnection = builder.Configuration.GetConnectionString("Postgres")!;
    var redisConnection = builder.Configuration.GetConnectionString("Redis")!;

    builder.Services.AddHealthChecks()
        .AddNpgSql(postgresConnection, name: "postgres")
        .AddRedis(redisConnection, name: "redis")
        .AddDbContextCheck<ValgorDbContext>("ef-core");

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors("Admin");

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Valgor API {AppVersion}");
    });

    app.UseAuthentication();
    app.UseAuthorization();

    var applyMigrations = app.Environment.IsDevelopment()
        || app.Configuration.GetValue("Database:ApplyMigrations", false);
    var seedData = app.Environment.IsDevelopment()
        || app.Configuration.GetValue("Database:Seed", false);

    await DatabaseInitializer.InitializeAsync(app.Services, applyMigrations, seedData);

    app.MapGet("/health", () => Results.Ok(new HealthResponse("ok", AppVersion)))
        .WithName("GetHealth")
        .WithTags("System")
        .AllowAnonymous()
        .Produces<HealthResponse>(StatusCodes.Status200OK);

    app.MapGet("/version", (IHostEnvironment environment) =>
            Results.Ok(new VersionResponse(AppVersion, ProductName, environment.EnvironmentName, DateTime.UtcNow)))
        .WithName("GetVersion")
        .WithTags("System")
        .AllowAnonymous()
        .Produces<VersionResponse>(StatusCodes.Status200OK);

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();

    app.MapControllers();

    Log.Information("Valgor API {Version} starting in {Environment}", AppVersion, app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Valgor API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
