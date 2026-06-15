using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IRoleRepository
    {
        Task<UserRole?> GetByNameAsync(UserRoleEnum name);
    }
}