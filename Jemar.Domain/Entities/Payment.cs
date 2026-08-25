using System;

namespace Jemar.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public decimal Amount { get; set; }
        public int PaymentStatusId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = null!;
        public string? PreferenceId { get; set; }
        public string? MercadoPagoPaymentId { get; set; }
        public string? StatusDetail { get; set; }
    }
}
