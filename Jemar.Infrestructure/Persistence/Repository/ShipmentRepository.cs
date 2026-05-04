using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly JemarDbContext _context;

        public ShipmentRepository(JemarDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Shipment shipment)
        {
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Shipment>> GetAllAsync()
        {
            return await _context.Shipments
                .Include(x => x.ShipmentType)
                .Include(x => x.ShipmentStatus)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments
                .Include(x => x.ShipmentType)
                .Include(x => x.ShipmentStatus)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
