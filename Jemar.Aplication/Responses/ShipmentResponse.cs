namespace Jemar.Aplication.Responses
{
    public class ShipmentResponse // DTO utilizado para devolver información de un Shipment al cliente
    {
        public Guid Id { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ShipmentType { get; set; } = string.Empty;
        public string ShipmentStatus { get; set; } = string.Empty;
    }
}
