using TaskManagment.Domain.Models;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Domain.RepositoryContracts
{
    public interface ITaskRepository
    {
        Task<TaskItem> Create(TaskItem task);
        Task<TaskItem?> GetById(Guid id);
        Task<IEnumerable<TaskItem>> GetAll();
        Task<IEnumerable<TaskItem>> GetByUserId(Guid userId);
        Task<TaskItem?> Update(TaskItem task);
        Task<TaskItem?> UpdateStatus(Guid id, TaskStatus status);
        Task Delete(Guid id);
    }
}
