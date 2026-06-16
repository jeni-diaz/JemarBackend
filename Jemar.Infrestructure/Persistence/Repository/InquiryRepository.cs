using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class InquiryRepository : BaseRepository<Inquiry>, IInquiryRepository
    {
        public InquiryRepository(JemarDbContext context) : base(context)
        {
        }

        public override async Task<List<Inquiry>> GetAllAsync()
        {
            return await _context.Inquiries
                .Include(i => i.Client)
                .Include(i => i.Employee)
                .Where(i => !i.IsDeleted)
                .ToListAsync();
        }

        public override async Task<Inquiry?> GetByIdAsync(Guid id)
        {
            return await _context.Inquiries
                .Include(i => i.Client)
                .Include(i => i.Employee)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<List<Inquiry>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Inquiries
                .Include(i => i.Client)
                .Include(i => i.Employee)
                .Where(i => i.ClientId == clientId && !i.IsDeleted)
                .ToListAsync();
        }
    }
}
