using Microsoft.EntityFrameworkCore;
using TaskManagment.Domain.Models;
using TaskManagment.Infrastructure.Data;

namespace TaskManagment.Infrastructure.Repostories
{
    public class TaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> Create(TaskItem task)
        {
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> GetById(Guid id)
        {
            return await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetAll()
        {
            return await _context.TaskItems.ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByUserId(Guid userId)
        {
            return await _context.TaskItems.Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<TaskItem?> Update(TaskItem task)
        {
            var existing = await GetById(task.Id);
            if (existing != null)
            {
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Status = task.Status;
                existing.Priority = task.Priority;
                await _context.SaveChangesAsync();
                return existing;
            }
            return null;
        }

        public async Task<TaskItem?> UpdateStatus(Guid id, Domain.Models.TaskStatus status)
        {
            var existing = await GetById(id);
            if (existing != null)
            {
                existing.Status = status;
                await _context.SaveChangesAsync();
                return existing;
            }
            return null;
        }

        public async Task Delete(Guid id)
        {
            var task = await GetById(id);
            if (task != null)
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}
