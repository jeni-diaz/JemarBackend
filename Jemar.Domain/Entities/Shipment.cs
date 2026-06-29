using System;

namespace Jemar.Domain.Entities
{
    public class Shipment : BaseEntity
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ShipmentTypeId { get; set; }
        public ShipmentType? ShipmentType { get; set; }
        public int ShipmentStatusId { get; set; }
        public ShipmentStatus? ShipmentStatus { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public int CreatedByRoleId { get; set; }
        public Role CreatedByRole { get; set; } = null!;
        public Guid? OnBehalfOfClientId { get; set; }
        public User? OnBehalfOfClient { get; set; }
    }
}