namespace Jemar.Aplication.Requests
{
    public class CreateShipmentRequest // DTO utilizado para crear un nuevo envío
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ShipmentTypeId { get; set; }
    }
}
