using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IRoleRepository
    {
        Task<Domain.Entities.UserRole?> GetByNameAsync(Domain.Enums.UserRoleEnum name);
    }
}
