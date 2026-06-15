using Microsoft.EntityFrameworkCore;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using System;

namespace Jemar.Infrastructure.Persistence
{
    public class JemarDbContext : DbContext
    {
        public JemarDbContext(DbContextOptions<JemarDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentType> ShipmentTypes { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Client>("Client")
                .HasValue<Employee>("Employee")
                .HasValue<SuperAdmin>("SuperAdmin");


            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Shipments)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.AssignedShipments)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);


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


            modelBuilder.Entity<ShipmentType>().HasData(
                new ShipmentType { Id = 1, Name = ShipmentTypeEnum.Express, Description = "Express shipment (24h)" },
                new ShipmentType { Id = 2, Name = ShipmentTypeEnum.Standar, Description = "Standard shipment (3-5 days)" }
            );
        }
    }
}