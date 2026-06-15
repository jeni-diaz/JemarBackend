namespace Jemar.Domain.Entities
{
    public class Shipment : BaseEntity
    {
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int ShipmentTypeId { get; set; }
        public ShipmentType? ShipmentType { get; set; }

        public int ShipmentStatusId { get; set; }
        public ShipmentStatus? ShipmentStatus { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public Guid? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
