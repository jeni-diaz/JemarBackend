using System;

namespace Jemar.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid? ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }
        public decimal Amount { get; set; }
        public int PaymentStatusId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = null!;
        public string? PreferenceId { get; set; }
        public string? MercadoPagoPaymentId { get; set; }
        public string? StatusDetail { get; set; }

        // Snapshot of the create-shipment request, used to actually create the
        // Shipment once Mercado Pago confirms the payment (the shipment does not
        // exist before that point).
        public Guid PendingShipmentId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByRole { get; set; } = string.Empty;
        public string PendingOrigin { get; set; } = string.Empty;
        public string PendingDestination { get; set; } = string.Empty;
        public int PendingShipmentTypeId { get; set; }
        public int PendingPackageSizeId { get; set; }
        public Guid? PendingOnBehalfOfClientId { get; set; }
    }
}
