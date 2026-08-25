using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IMercadoPagoService
    {
        Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(Guid referenceId, decimal amount, string title, string frontendBaseUrl, string backendBaseUrl);
        Task<MercadoPagoPaymentInfo?> GetPaymentAsync(long mercadoPagoPaymentId);
    }
}
