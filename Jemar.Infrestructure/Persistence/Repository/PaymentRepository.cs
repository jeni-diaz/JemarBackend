using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(JemarDbContext context) : base(context)
        {
        }

        public override async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Shipment)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public override async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Shipment)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }
    }
}
