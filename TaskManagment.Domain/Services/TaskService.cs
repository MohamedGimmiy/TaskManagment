using TaskManagment.Domain.Models;
using TaskManagment.Domain.RepositoryContracts;
using TaskManagment.Domain.ServicesContract;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Domain.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskItem> CreateTask(Guid userId, string title, string? description, string? priority)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description ?? string.Empty,
                Status = TaskStatus.Pending,
                Priority = priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            return await _taskRepository.Create(task);
        }

        public async Task<TaskItem?> GetTaskById(Guid taskId, Guid currentUserId, string currentUserRole)
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

            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetUserTasks(Guid currentUserId, string currentUserRole)
        {
            if (currentUserRole == "Admin")
            {
                return await _taskRepository.GetAll();
            }

            return await _taskRepository.GetByUserId(currentUserId);
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

            return await _taskRepository.UpdateStatus(taskId, newStatus);
        }
    }
}
