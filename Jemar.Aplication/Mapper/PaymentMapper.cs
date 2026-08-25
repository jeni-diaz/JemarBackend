using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;

namespace Jemar.Aplication.Mapper
{
    public static class PaymentMapper
    {
        public static PaymentStatusResponse ToPaymentStatusResponse(this Payment payment)
        {
            return new PaymentStatusResponse
            {
                ShipmentId = payment.ShipmentId,
                Status = payment.PaymentStatus.Name.ToString(),
                StatusDetail = payment.StatusDetail,
                Amount = payment.Amount,
                UpdatedAt = payment.UpdatedDateTime
            };
        }
    }
}
