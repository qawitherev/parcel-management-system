using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Core.BackgroundServices;

/// <summary>
/// Enqueues parcel-arrived notifications to be processed asynchronously.
/// Mirrors the pattern used by ParcelOverstayBackgroundService:
///   - Implements both BackgroundService (to dequeue) and INotificationEnqueuer (to enqueue)
///   - Resolves scoped services via IServiceScopeFactory inside each work item
/// </summary>
public interface INotificationEnqueuer
{
    ValueTask EnqueueParcelArrivedNotification(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName);
}

public class NotificationBackgroundService : BackgroundService, INotificationEnqueuer
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationBackgroundService(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
    }

    // ── Enqueuer ────────────────────────────────────────

    public ValueTask EnqueueParcelArrivedNotification(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName)
    {
        // Capture locals for the closure to avoid modified-closure bugs
        var pid = parcelId;
        var tn = trackingNumber;
        var ruid = residentUnitId;
        var ln = lockerName;

        return _taskQueue.QueueBackgroundTaskAsync(async ct =>
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            var notificationService = scope.ServiceProvider
                .GetRequiredService<IParcelNotificationService>();
            await notificationService.NotifyResidentsOfParcelArrivalAsync(
                pid, tn, ruid, ln, ct);
        });
    }

    // ── BackgroundService ───────────────────────────────

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                // Fire and forget — a failed notification shouldn't crash the loop
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[NotificationBackgroundService] Error processing notification: {ex.Message}");
                    }
                }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
