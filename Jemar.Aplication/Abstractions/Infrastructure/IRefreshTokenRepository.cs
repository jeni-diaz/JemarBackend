using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
    }
}
