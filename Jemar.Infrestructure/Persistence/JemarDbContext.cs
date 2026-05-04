using Microsoft.EntityFrameworkCore;
using Jemar.Domain.Entities;

namespace Jemar.Infrastructure.Persistence
{
    public class JemarDbContext : DbContext
    {
        public JemarDbContext(DbContextOptions<JemarDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentType> ShipmentTypes { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }
    }
}