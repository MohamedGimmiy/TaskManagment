using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManagment.Domain.Models;
using TaskManagment.Domain.RepositoryContracts;
using TaskManagment.Domain.ServicesContract;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Domain.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository taskRepository, IDistributedCache cache, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TaskItem> CreateTask(Guid userId, string title, string? description, int priority = 2)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description ?? string.Empty,
                Status = TaskStatus.Pending,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            return await _taskRepository.Create(task);
        }

        public async Task<TaskItem?> GetTaskById(Guid taskId, Guid currentUserId, string currentUserRole)
        {
            var cacheKey = $"task:{taskId}";
            var cachedBytes = await _cache.GetAsync(cacheKey);

            TaskItem? task;

            if (cachedBytes != null)
            {
                _logger.LogInformation("Cache HIT for task {TaskId}. Returning data from Redis.", taskId);
                task = JsonSerializer.Deserialize<TaskItem>(cachedBytes);
                if (task == null)
                {
                    return null;
                }
            }
            else
            {
                _logger.LogInformation("Cache MISS for task {TaskId}. Loading from database.", taskId);
                task = await _taskRepository.GetById(taskId);
                if (task == null)
                {
                    return null;
                }

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                var serialized = JsonSerializer.SerializeToUtf8Bytes(task);
                await _cache.SetAsync(cacheKey, serialized, options);
            }

            if (currentUserRole != "Admin" && task.UserId != currentUserId)
            {
                return null;
            }

            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetUserTasks(Guid currentUserId, string currentUserRole)
        {
            IEnumerable<TaskItem> tasks;

            if (currentUserRole == "Admin")
            {
                tasks = await _taskRepository.GetAll();
            }
            else
            {
                tasks = await _taskRepository.GetByUserId(currentUserId);
            }

            return tasks.OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt);
        }

        public async Task<TaskItem?> UpdateTaskStatus(Guid taskId, TaskStatus newStatus, Guid currentUserId, string currentUserRole)
        {
            var task = await _taskRepository.GetById(taskId);

            if (task == null)
            {
                return null;
            }

            if (currentUserRole != "Admin" && task.UserId != currentUserId)
            {
                return null;
            }

            var updated = await _taskRepository.UpdateStatus(taskId, newStatus);

            if (updated != null)
            {
                await _cache.RemoveAsync($"task:{taskId}");
            }

            return updated;
        }
    }
}
