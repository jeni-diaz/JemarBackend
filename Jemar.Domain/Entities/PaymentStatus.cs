using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class PaymentStatus
    {
        public int Id { get; set; }
        public PaymentStatusEnum Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
