using System.ComponentModel.DataAnnotations;

namespace Jemar.Aplication.Requests
{
    public class UpdateShipmentRequest
    {
        [Required(ErrorMessage = "El estado del envío es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El estado del envío no es válido.")]
        public int ShipmentStatusId { get; set; }
    }
}