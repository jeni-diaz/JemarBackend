using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IPaymentService
    {
        Task<CreatePaymentPreferenceResponse> CreatePreferenceAsync(Guid shipmentId, Guid currentUserId, string currentUserRole, string frontendBaseUrl, string backendBaseUrl);
        Task<PaymentStatusResponse> GetStatusAsync(Guid shipmentId, Guid currentUserId, string currentUserRole);
        Task<PaymentStatusResponse?> SyncFromMercadoPagoAsync(long mercadoPagoPaymentId, Guid? currentUserId = null, string? currentUserRole = null);
    }
}
