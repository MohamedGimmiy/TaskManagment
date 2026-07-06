using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagment.Domain.Models;
using TaskManagment.Domain.ServicesContract;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Domain.Services
{
    public class TaskBackgroundService : BackgroundService
    {
        private readonly ITaskProcessingQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskBackgroundService> _logger;

        public TaskBackgroundService(
            ITaskProcessingQueue taskQueue,
            IServiceProvider serviceProvider,
            ILogger<TaskBackgroundService> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var taskId = await _taskQueue.DequeueTask(stoppingToken);
                    
                    if (taskId.HasValue)
                    {
                        await ProcessTaskAsync(taskId.Value, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing task in background service");
                }
            }

            _logger.LogInformation("Task Background Service is stopping.");
        }

        private async Task ProcessTaskAsync(Guid taskId, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            
            var task = await context.Set<TaskItem>().FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
            
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", taskId);
                return;
            }

            _logger.LogInformation("Processing task {TaskId}: {TaskTitle}", taskId, task.Title);

            // Simulate background processing work
            await Task.Delay(2000, cancellationToken); // Simulate 2 seconds of work

            // Update task status to InProgress
            task.Status = TaskStatus.InProgress;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task {TaskId} status updated to InProgress", taskId);

            // Simulate more work
            await Task.Delay(3000, cancellationToken); // Simulate 3 more seconds of work

            // Update task status to Done
            task.Status = TaskStatus.Done;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task {TaskId} status updated to Done", taskId);
        }
    }
}
