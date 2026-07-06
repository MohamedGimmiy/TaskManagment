using TaskManagment.Domain.Models;

namespace TaskManagment.Domain.RepositoryContracts
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> Create(RefreshToken refreshToken);
        Task<RefreshToken?> GetByToken(string token);
        Task Revoke(Guid id);
    }
}
