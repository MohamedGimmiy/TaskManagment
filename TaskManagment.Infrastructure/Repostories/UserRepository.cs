using Microsoft.EntityFrameworkCore;
using TaskManagment.Domain.Models;
using TaskManagment.Domain.RepositoryContracts;
using TaskManagment.Infrastructure.Data;

namespace TaskManagment.Infrastructure.Repostories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> Update(User user)
        {
            var existing = await GetById(user.Id);
            if (existing != null)
            {
                existing.Name = user.Name;
                existing.Email = user.Email;
                existing.Password = user.Password;
                existing.Role = user.Role;
                await _context.SaveChangesAsync();
                return existing;
            }
            return null;
        }

        public async Task Delete(Guid id)
        {
            var user = await GetById(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
