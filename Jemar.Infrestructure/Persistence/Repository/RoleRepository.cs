using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly JemarDbContext _context;

        public RoleRepository(JemarDbContext context)
        {
            _context = context;
        }

        public async Task<UserRole?> GetByNameAsync(UserRoleEnum name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }
    }
}
