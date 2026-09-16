using System;
using System.ComponentModel.DataAnnotations;

namespace Jemar.Aplication.Requests
{
    public class CreateShipmentRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "La dirección de origen es requerida.")]
        [MinLength(5, ErrorMessage = "La dirección de origen debe tener al menos 5 caracteres.")]
        public string Origin { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección de destino es requerida.")]
        [MinLength(5, ErrorMessage = "La dirección de destino debe tener al menos 5 caracteres.")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de envío es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de envío no es válido.")]
        public int ShipmentTypeId { get; set; }

        [Required(ErrorMessage = "El tamaño del paquete es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El tamaño del paquete no es válido.")]
        public int PackageSizeId { get; set; }
        public Guid? OnBehalfOfClientId { get; set; }
    }
}