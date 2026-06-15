namespace Jemar.Domain.Entities
{
    public class Client : User
    {
        public DateTime RegistrationDate { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
