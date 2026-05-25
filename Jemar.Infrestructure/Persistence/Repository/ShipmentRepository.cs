using Jemar.Application.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Jemar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class ShipmentRepository
        : BaseRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(JemarDbContext context)
            : base(context)
        {
        }

        public override List<Shipment> GetAll()
        {
            return _dbSet
                .Include(x => x.ShipmentType)
                .Include(x => x.ShipmentStatus)
                .Where(x => !x.IsDeleted)
                .ToList();
        }

        public override Shipment? GetById(Guid id)
        {
            return _dbSet
                .Include(x => x.ShipmentType)
                .Include(x => x.ShipmentStatus)
                .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }
    }
}