using Jemar.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
