using TaskManagment.Domain.Models;
using TaskManagment.Domain.RepositoryContracts;
using TaskManagment.Domain.ServicesContract;

namespace TaskManagment.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User?> GetUserById(Guid id, Guid currentUserId, string currentUserRole)
        {
            var user = await _userRepository.GetById(id);

            if (user == null)
            {
                return null;
            }

            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> GetProfile(Guid currentUserId)
        {
            return await _userRepository.GetById(currentUserId);
        }

        public async Task<User> CreateUser(string name, string email, string password, string role)
        {
            var existing = await _userRepository.GetByEmail(email);
            if (existing != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
            {
                throw new InvalidOperationException($"Invalid role. Valid values: {string.Join(", ", Enum.GetNames<UserRole>())}");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = parsedRole,
                CreatedAt = DateTime.UtcNow
            };

            return await _userRepository.Create(user);
        }

        public async Task DeleteUser(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                await _userRepository.Delete(id);
            }
        }
    }
}
