using Microsoft.EntityFrameworkCore;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Infrastructure.Persistence
{
    public class JemarDbContext : DbContext
    {
        public JemarDbContext(DbContextOptions<JemarDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentType> ShipmentTypes { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Client)
                .WithMany(u => u.Shipments)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Employee)
                .WithMany(u => u.AssignedShipments)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Client)
                .WithMany(u => u.Inquiries)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Employee)
                .WithMany(u => u.AssignedInquiries)
                .HasForeignKey(i => i.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = UserRole.Client, Description = "Client Role" },
                new Role { Id = 2, Name = UserRole.Employee, Description = "Employee Role" },
                new Role { Id = 3, Name = UserRole.SuperAdmin, Description = "Super Admin Role" }
            );

            modelBuilder.Entity<ShipmentStatus>().HasData(
                new ShipmentStatus { Id = 1, Name = ShipmentStatusEnum.Pending, Description = "Shipment is pending" },
                new ShipmentStatus { Id = 2, Name = ShipmentStatusEnum.In_transit, Description = "Shipment is in transit" },
                new ShipmentStatus { Id = 3, Name = ShipmentStatusEnum.Delivered, Description = "Shipment has been delivered" },
                new ShipmentStatus { Id = 4, Name = ShipmentStatusEnum.Cancelled, Description = "Shipment has been cancelled" }
            );

            modelBuilder.Entity<ShipmentType>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShipmentType>().HasData(
                new ShipmentType { Id = 1, Name = ShipmentTypeEnum.Express, Description = "Express shipment (24h)", Price = 3000.00m },
                new ShipmentType { Id = 2, Name = ShipmentTypeEnum.Standard, Description = "Standard shipment (3-5 days)", Price = 1500.00m }
            );
        }
    }
}