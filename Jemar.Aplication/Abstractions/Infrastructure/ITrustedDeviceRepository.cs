using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface ITrustedDeviceRepository : IBaseRepository<TrustedDevice>
    {
        Task<List<TrustedDevice>> GetActiveByUserIdAsync(Guid userId);
    }
}
