using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IPaymentService
    {
        Task<CreatePaymentPreferenceResponse> CreateCheckoutAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole, string frontendBaseUrl, string backendBaseUrl);
        Task<PaymentStatusResponse?> SyncFromMercadoPagoAsync(long mercadoPagoPaymentId, Guid? currentUserId = null, string? currentUserRole = null);
    }
}
