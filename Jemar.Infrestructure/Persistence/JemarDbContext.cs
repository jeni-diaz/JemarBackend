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
        public DbSet<Domain.Entities.UserRole> Roles { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
        public DbSet<ShipmentType> ShipmentTypes { get; set; }
        public DbSet<ShipmentStatus> ShipmentStatuses { get; set; }
        public DbSet<PackageSize> PackageSizes { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrustedDevice>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(t => t.TokenHash)
                .IsUnique();

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.CreatedByUser)
                .WithMany(u => u.CreatedShipments)
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.OnBehalfOfClient)
                .WithMany(u => u.OnBehalfShipments)
                .HasForeignKey(s => s.OnBehalfOfClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.CreatedByRole)
                .WithMany()
                .HasForeignKey(s => s.CreatedByRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.PackageSize)
                .WithMany()
                .HasForeignKey(s => s.PackageSizeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShipmentStatusHistory>()
                .HasOne(h => h.Shipment)
                .WithMany()
                .HasForeignKey(h => h.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShipmentStatusHistory>()
                .HasOne(h => h.ShipmentStatus)
                .WithMany()
                .HasForeignKey(h => h.ShipmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShipmentStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.DistanceKm)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.CreatedByUser)
                .WithMany(u => u.CreatedInquiries)
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.RespondedByUser)
                .WithMany(u => u.RespondedInquiries)
                .HasForeignKey(i => i.RespondedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Shipment)
                .WithMany()
                .HasForeignKey(p => p.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany()
                .HasForeignKey(p => p.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, Name = UserRoleEnum.Client, Description = "Client Role" },
                new UserRole { Id = 2, Name = UserRoleEnum.Employee, Description = "Employee Role" },
                new UserRole { Id = 3, Name = UserRoleEnum.SuperAdmin, Description = "Super Admin Role" }
            );

            modelBuilder.Entity<ShipmentStatus>().HasData(
                new ShipmentStatus { Id = 1, Name = ShipmentStatusEnum.Pending, Description = "Shipment is pending" },
                new ShipmentStatus { Id = 2, Name = ShipmentStatusEnum.InTransit, Description = "Shipment is in transit" },
                new ShipmentStatus { Id = 3, Name = ShipmentStatusEnum.Delivered, Description = "Shipment has been delivered" },
                new ShipmentStatus { Id = 4, Name = ShipmentStatusEnum.Cancelled, Description = "Shipment has been cancelled" }
            );

            modelBuilder.Entity<ShipmentType>().HasData(
                new ShipmentType { Id = 1, Name = ShipmentTypeEnum.Express, Description = "Express shipment (24h)" },
                new ShipmentType { Id = 2, Name = ShipmentTypeEnum.Standard, Description = "Standard shipment (3-5 days)" }
            );

            modelBuilder.Entity<PackageSize>(entity =>
            {
                entity.Property(p => p.MaxLengthCm).HasPrecision(18, 2);
                entity.Property(p => p.MaxWidthCm).HasPrecision(18, 2);
                entity.Property(p => p.MaxHeightCm).HasPrecision(18, 2);
                entity.Property(p => p.BasePrice).HasPrecision(18, 2);
                entity.Property(p => p.RatePerKm).HasPrecision(18, 2);
                entity.Property(p => p.Surcharge).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PackageSize>().HasData(
                new PackageSize { Id = 1, Name = PackageSizeEnum.Small, MaxLengthCm = 30.00m, MaxWidthCm = 30.00m, MaxHeightCm = 30.00m, BasePrice = 1500.00m, RatePerKm = 20.00m, Surcharge = 0.00m },
                new PackageSize { Id = 2, Name = PackageSizeEnum.Medium, MaxLengthCm = 60.00m, MaxWidthCm = 60.00m, MaxHeightCm = 60.00m, BasePrice = 2500.00m, RatePerKm = 35.00m, Surcharge = 1000.00m },
                new PackageSize { Id = 3, Name = PackageSizeEnum.Large, MaxLengthCm = 120.00m, MaxWidthCm = 120.00m, MaxHeightCm = 120.00m, BasePrice = 4000.00m, RatePerKm = 50.00m, Surcharge = 2500.00m }
            );

            modelBuilder.Entity<PaymentStatus>().HasData(
                new PaymentStatus { Id = 1, Name = PaymentStatusEnum.Pending, Description = "Payment is pending" },
                new PaymentStatus { Id = 2, Name = PaymentStatusEnum.Approved, Description = "Payment was approved" },
                new PaymentStatus { Id = 3, Name = PaymentStatusEnum.Rejected, Description = "Payment was rejected" },
                new PaymentStatus { Id = 4, Name = PaymentStatusEnum.Cancelled, Description = "Payment was cancelled" }
            );
        }
    }
}