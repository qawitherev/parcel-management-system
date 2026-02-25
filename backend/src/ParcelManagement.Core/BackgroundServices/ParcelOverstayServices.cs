using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Core.BackgroundServices
{
    public interface IParcelOverstayEnqueuer
    {
        ValueTask EnqueueProcessParcelOverstay();
    }

    public class ParcelOverstayService(IServiceScopeFactory serviceScopeFactory, IBackgroundTaskQueue backgroundTaskQueue) : BackgroundService, IParcelOverstayEnqueuer
    {
        private readonly CronExpression _cronExpression = CronExpression.Daily;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var c = RunCronJob(stoppingToken);
            var s = PickupJobQueue(stoppingToken);
            await Task.WhenAll(c, s);
        }

        private async Task RunCronJob(CancellationToken ct)
        {
            Console.WriteLine("Cronjob activated");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var utcNow = DateTime.UtcNow;
                    var nextRun = _cronExpression.GetNextOccurrence(utcNow);
                    if (!nextRun.HasValue) break;

                    var delayAmount = nextRun.Value - utcNow;
                    await Task.Delay(delayAmount, ct);
                    await backgroundTaskQueue.QueueBackgroundTaskAsync(ProcessParcelOverstay);
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

        public ValueTask EnqueueProcessParcelOverstay() 
            => backgroundTaskQueue.QueueBackgroundTaskAsync(ProcessParcelOverstay);
    }
}