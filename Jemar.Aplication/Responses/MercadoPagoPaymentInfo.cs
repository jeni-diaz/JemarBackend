namespace Jemar.Aplication.Responses
{
    public class MercadoPagoPaymentInfo
    {
        public long Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusDetail { get; set; }
        public string? ExternalReference { get; set; }
        public decimal? TransactionAmount { get; set; }
    }
}
