using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Core.BackgroundServices
{
    public class ParcelOverstayService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
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
                        if (stoppingToken.IsCancellationRequested) return;
                        await ProcessParcelOverstay(stoppingToken);
                    }
                } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessParcelOverstay(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateAsyncScope())
                {
                    var parcelService = scope.ServiceProvider.GetRequiredService<IParcelService>();
                    await parcelService.UpdateOverstayedParcel();
                }
            } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}