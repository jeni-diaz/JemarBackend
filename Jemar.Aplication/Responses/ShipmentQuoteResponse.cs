using System;

namespace Jemar.Aplication.Responses
{
    public class ShipmentQuoteResponse
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public decimal DistanceKm { get; set; }
    }
}
