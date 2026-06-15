using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class ShipmentRepository : BaseRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(JemarDbContext context) : base(context)
        {
        }

        public override async Task<List<Shipment>> GetAllAsync()
        {
            return await _context.Shipments
                .Include(s => s.ShipmentType)
                .Include(s => s.ShipmentStatus)
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public override async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments
                .Include(s => s.ShipmentType)
                .Include(s => s.ShipmentStatus)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<List<Shipment>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Shipments
                .Include(s => s.ShipmentType)
                .Include(s => s.ShipmentStatus)
                .Where(s => s.ClientId == clientId && !s.IsDeleted)
                .ToListAsync();
        }
    }
}