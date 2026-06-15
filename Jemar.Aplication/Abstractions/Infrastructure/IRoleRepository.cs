using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(UserRole name);
    }
}
