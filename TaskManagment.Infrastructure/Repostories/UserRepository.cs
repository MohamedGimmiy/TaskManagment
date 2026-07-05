using TaskManagment.Domain.Models;

namespace TaskManagment.Infrastructure.Repostories
{
    public class UserRepository
    {
        private readonly List<User> _users = new();

        public User Create(User user)
        {
            _users.Add(user);
            return user;
        }

        public User? GetById(Guid id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User Update(User user)
        {
            var existing = GetById(user.Id);
            if (existing != null)
            {
                existing.Name = user.Name;
                existing.Email = user.Email;
                existing.Password = user.Password;
                existing.Role = user.Role;
            }
            return existing ?? user;
        }

        public void Delete(Guid id)
        {
            var user = GetById(id);
            if (user != null)
            {
                _users.Remove(user);
            }
        }
    }
}
