namespace Jemar.Domain.Entities
{
    public class Employee : User
    {
        public DateTime HireDate { get; set; }
        public string Position { get; set; } = string.Empty;

        public ICollection<Shipment> AssignedShipments { get; set; } = new List<Shipment>();

    }
}
