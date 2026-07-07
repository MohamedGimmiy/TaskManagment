using System.Collections.Concurrent;
using TaskManagment.Domain.ServicesContract;

namespace TaskManagment.Infrastructure.Services
{
    public class TaskProcessingQueue : ITaskProcessingQueue
    {
        private readonly ConcurrentQueue<Guid> _taskQueue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void EnqueueTask(Guid taskId)
        {
            _taskQueue.Enqueue(taskId);
            _signal.Release();
        }

        public async Task<Guid?> DequeueTask(System.Threading.CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_taskQueue.TryDequeue(out var taskId))
            {
                return taskId;
            }
            return null;
        }
    }
}
