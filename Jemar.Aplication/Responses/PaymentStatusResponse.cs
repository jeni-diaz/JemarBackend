using System;

namespace Jemar.Aplication.Responses
{
    public class PaymentStatusResponse
    {
        public Guid ShipmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusDetail { get; set; }
        public decimal Amount { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
