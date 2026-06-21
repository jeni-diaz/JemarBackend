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

        public Guid ClientId { get; set; }
        public User Client { get; set; } = null!;

        public Guid? EmployeeId { get; set; }
        public User? Employee { get; set; }
    }
}