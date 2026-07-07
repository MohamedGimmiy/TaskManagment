using TaskManagment.Domain.Models;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Domain.ServicesContract
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTask(Guid userId, string title, string? description, int priority = 2);
        Task<TaskItem?> GetTaskById(Guid taskId, Guid currentUserId, string currentUserRole);
        Task<IEnumerable<TaskItem>> GetUserTasks(Guid currentUserId, string currentUserRole);
        Task<TaskItem?> UpdateTaskStatus(Guid taskId, TaskStatus newStatus, Guid currentUserId, string currentUserRole);
    }
}
