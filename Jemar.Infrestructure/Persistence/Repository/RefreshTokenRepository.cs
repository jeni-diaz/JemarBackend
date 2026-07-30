using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(JemarDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsDeleted);
        }

        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsDeleted && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
