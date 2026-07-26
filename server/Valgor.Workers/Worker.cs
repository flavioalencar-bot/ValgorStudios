using Valgor.Application;
using Valgor.Infrastructure;

namespace Valgor.Workers;

/// <summary>
/// Host de processamento assíncrono do Valgor. Jobs de domínio serão registrados como IHostedService adicionais.
/// </summary>
public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Valgor worker host online");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
