using System.Threading.Channels;

namespace ParcelManagement.Core.BackgroundServices
{
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

        public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        {
            Console.WriteLine("Item picked up for processing");
            return await _queue.Reader.ReadAsync(ct);
        }

        public async ValueTask QueueBackgroundTaskAsync(Func<CancellationToken, Task> workItem)
        {
            Console.WriteLine("Work item added to queue");
            await _queue.Writer.WriteAsync(workItem);
        }
    }
}