using Jemar.Domain.Entities;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IMercadoPagoService
    {
        Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(Shipment shipment, string frontendBaseUrl, string backendBaseUrl);
        Task<MercadoPagoPaymentInfo?> GetPaymentAsync(long mercadoPagoPaymentId);
    }
}
