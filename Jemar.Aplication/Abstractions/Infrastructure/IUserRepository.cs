using Jemar.Domain.Entities;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}