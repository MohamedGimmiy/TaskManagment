using TaskManagment.Domain.Models;

namespace TaskManagment.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<User> Create(User user);
        Task<User?> GetById(Guid id);
        Task<User?> GetByEmail(string email);
        Task<IEnumerable<User>> GetAll();
        Task<User?> Update(User user);
        Task Delete(Guid id);
    }
}
