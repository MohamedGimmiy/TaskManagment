namespace TaskManagment.Domain.ServicesContract
{
    public interface ITaskProcessingQueue
    {
        void EnqueueTask(Guid taskId);
        Task<Guid?> DequeueTask(System.Threading.CancellationToken cancellationToken);
    }
}
