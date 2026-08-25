using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
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

        public async Task<List<Payment>> GetByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Where(p => p.ShipmentId == shipmentId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDateTime)
                .ToListAsync();
        }

        public async Task<Payment?> GetLatestByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Where(p => p.ShipmentId == shipmentId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDateTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Payment?> GetPendingByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Where(p => p.ShipmentId == shipmentId && !p.IsDeleted && p.PaymentStatusId == (int)PaymentStatusEnum.Pending)
                .OrderByDescending(p => p.CreatedDateTime)
                .FirstOrDefaultAsync();
        }
    }
}
