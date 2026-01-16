using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;
using Cronos;
using Microsoft.Extensions.DependencyInjection;

namespace ParcelManagement.Core.BackgroundServices
{
    public class RemoveSessionBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        // Todo: to move the cron expression into config file 
        private readonly CronExpression _cronExpression = CronExpression.Daily;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
            {
                var utcNow = DateTime.UtcNow;
                var nextRun = _cronExpression.GetNextOccurrence(utcNow);

                if (nextRun.HasValue)
                {
                    var delayAmount = nextRun.Value - utcNow;
                    await Task.Delay(delayAmount, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                        {
                            await CleanupExpiredSessions(stoppingToken);
                        }
                }
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            }
        }

        private async Task CleanupExpiredSessions(CancellationToken stoppingToken)
        {
            try
            {
                using(var scope = _scopeFactory.CreateAsyncScope())
            {
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                await sessionService.RemoveExpiredSessions();
            }
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}