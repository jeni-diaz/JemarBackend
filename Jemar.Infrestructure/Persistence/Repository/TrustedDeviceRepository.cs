using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class TrustedDeviceRepository : BaseRepository<TrustedDevice>, ITrustedDeviceRepository
    {
        public TrustedDeviceRepository(JemarDbContext context) : base(context)
        {
        }

        public async Task<List<TrustedDevice>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.TrustedDevices
                .Where(d => d.UserId == userId && !d.IsDeleted && d.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
