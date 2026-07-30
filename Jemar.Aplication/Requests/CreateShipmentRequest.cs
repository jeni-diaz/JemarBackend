using System;
using System.ComponentModel.DataAnnotations;

namespace Jemar.Aplication.Requests
{
    public class CreateShipmentRequest
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Origin address is required.")]
        [MinLength(5, ErrorMessage = "Origin address must be at least 5 characters long.")]
        public string Origin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination address is required.")]
        [MinLength(5, ErrorMessage = "Destination address must be at least 5 characters long.")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipment type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid shipment type ID.")]
        public int ShipmentTypeId { get; set; }

        [Required(ErrorMessage = "Package size is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid package size ID.")]
        public int PackageSizeId { get; set; }
        public Guid? OnBehalfOfClientId { get; set; }
    }
}