using TaskManagment.Domain.Models;

namespace TaskManagment.Infrastructure.Repostories
{
    public class TaskRepository
    {
        private readonly List<TaskItem> _tasks = new();

        public TaskItem Create(TaskItem task)
        {
            _tasks.Add(task);
            return task;
        }

        public TaskItem? GetById(Guid id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<TaskItem> GetAll()
        {
            return _tasks;
        }

        public IEnumerable<TaskItem> GetByUserId(Guid userId)
        {
            return _tasks.Where(t => t.UserId == userId);
        }

        public TaskItem Update(TaskItem task)
        {
            var existing = GetById(task.Id);
            if (existing != null)
            {
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Status = task.Status;
                existing.Priority = task.Priority;
            }
            return existing ?? task;
        }

        public void Delete(Guid id)
        {
            var task = GetById(id);
            if (task != null)
            {
                _tasks.Remove(task);
            }
        }
    }
}
