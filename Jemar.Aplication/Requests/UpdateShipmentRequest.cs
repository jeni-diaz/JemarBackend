using System.ComponentModel.DataAnnotations;

namespace Jemar.Aplication.Requests
{
    public class UpdateShipmentRequest
    {
        [Required(ErrorMessage = "Shipment status is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid shipment status ID.")]
        public int ShipmentStatusId { get; set; }
    }
}