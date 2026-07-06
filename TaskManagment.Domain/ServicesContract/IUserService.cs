using TaskManagment.Domain.Models;

namespace TaskManagment.Domain.ServicesContract
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User?> GetUserById(Guid id, Guid currentUserId, string currentUserRole);
        Task<User?> GetProfile(Guid currentUserId);
        Task<User> CreateUser(string name, string email, string password, string role);
        Task DeleteUser(Guid id);
    }
}
