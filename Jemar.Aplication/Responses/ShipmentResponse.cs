using System;

namespace Jemar.Aplication.Responses
{
    public class ShipmentResponse
    {
        public Guid Id { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal Price { get; set; }
        public string ShipmentType { get; set; } = string.Empty;
        public string ShipmentStatus { get; set; } = string.Empty;
        public string PackageSize { get; set; } = string.Empty;
        // Datos del cliente dueño del envío (para que el personal vea de quién es).
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public int CreatedByRoleId { get; set; }
        public Guid? OnBehalfOfClientId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
    }
}