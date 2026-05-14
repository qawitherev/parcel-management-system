using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Core.BackgroundServices
{
    public interface ISessionEnqueuer
    {
        ValueTask EnqueueCleanupSession(); 
    }

    public class SessionBackgroundService(IBackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory serviceScopeFactory) : BackgroundService, ISessionEnqueuer
    {
        private readonly CronExpression _cronExpression = CronExpression.Daily;

        public ValueTask EnqueueCleanupSession() => backgroundTaskQueue.QueueBackgroundTaskAsync(CleanupExpiredSession);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var c = RunCronJob(stoppingToken);
                    var s = PickupJobQueue(stoppingToken);
                    await Task.WhenAll(c, s);

                } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task RunCronJob(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var utcNow = DateTime.UtcNow;
                    var nextRun = _cronExpression.GetNextOccurrence(utcNow);
                    if (nextRun.HasValue)
                    {
                        var delayAmount = nextRun.Value - utcNow;
                        await Task.Delay(delayAmount, ct);
                        await backgroundTaskQueue.QueueBackgroundTaskAsync(CleanupExpiredSession);
                    }

                } catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task PickupJobQueue(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var workItem = await backgroundTaskQueue.DequeueAsync(ct);
                    await workItem(ct);
                    
                } catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task CleanupExpiredSession(CancellationToken ct)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateAsyncScope())
                {
                    var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                    await sessionService.RemoveExpiredSessions();
                }
            } catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
        }
    }
}