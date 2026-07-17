using Jemar.Domain.Entities;
using Jemar.Aplication.Abstractions.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
                .Include(s => s.PackageSize)
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public override async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments
                .Include(s => s.ShipmentType)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.PackageSize)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<List<Shipment>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Shipments
                .Include(s => s.ShipmentType)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.PackageSize)
                .Where(s => (s.CreatedByUserId == clientId || s.OnBehalfOfClientId == clientId) && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<ShipmentType?> GetShipmentTypeByIdAsync(int id)
        {
            return await _context.ShipmentTypes.FindAsync(id);
        }

        public async Task<PackageSize?> GetPackageSizeByIdAsync(int id)
        {
            return await _context.PackageSizes.FindAsync(id);
        }

        public async Task<List<ShipmentType>> GetShipmentTypesAsync()
        {
            return await _context.ShipmentTypes.ToListAsync();
        }

        public async Task<List<PackageSize>> GetPackageSizesAsync()
        {
            return await _context.PackageSizes.ToListAsync();
        }
    }
}