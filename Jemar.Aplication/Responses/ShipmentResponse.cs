using System;

namespace Jemar.Aplication.Responses
{
    public class ShipmentResponse
    {
        public Guid Id { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ShipmentType { get; set; } = string.Empty;
        public string ShipmentStatus { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public int CreatedByRoleId { get; set; }
        public Guid? OnBehalfOfClientId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
    }
}