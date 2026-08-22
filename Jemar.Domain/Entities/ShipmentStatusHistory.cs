using System;

namespace Jemar.Domain.Entities
{
    public class ShipmentStatusHistory : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public int ShipmentStatusId { get; set; }
        public ShipmentStatus ShipmentStatus { get; set; } = null!;
        public Guid ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; } = null!;
    }
}
