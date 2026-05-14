using System.Threading.Channels;

namespace ParcelManagement.Core.BackgroundServices;

/// <summary>
/// Channel-based background task queue.
/// Enables fire-and-forget async work (notifications, overstay processing, etc.)
/// without blocking the HTTP request pipeline.
/// </summary>
public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundTaskAsync(Func<CancellationToken, Task> workItem);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
    }

    public async ValueTask QueueBackgroundTaskAsync(Func<CancellationToken, Task> workItem)
    {
        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct)
    {
        return await _queue.Reader.ReadAsync(ct);
    }
}
