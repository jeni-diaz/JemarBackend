using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Error;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IConfiguration _configuration;

        public MercadoPagoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void EnsureAccessToken()
        {
            var accessToken = _configuration["MercadoPago:AccessToken"]
                ?? throw new InvalidOperationException("La configuración 'MercadoPago:AccessToken' no existe.");

            MercadoPago.Config.MercadoPagoConfig.AccessToken = accessToken;
        }

        public async Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(Shipment shipment, string frontendBaseUrl, string backendBaseUrl)
        {
            EnsureAccessToken();

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = $"Envío Jemar #{shipment.Id}",
                        Quantity = 1,
                        CurrencyId = "ARS",
                        UnitPrice = shipment.Price
                    }
                },
                ExternalReference = shipment.Id.ToString(),
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{frontendBaseUrl}/payment/success",
                    Pending = $"{frontendBaseUrl}/payment/pending",
                    Failure = $"{frontendBaseUrl}/payment/failure"
                },
                // Mercado Pago requiere back_urls en HTTPS para poder usar auto_return;
                // en desarrollo local (http://localhost) se omite y el comprador vuelve manualmente.
                AutoReturn = frontendBaseUrl.StartsWith("https://") ? "approved" : null,
                NotificationUrl = $"{backendBaseUrl}/api/payment/webhook"
            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(request);

            return (preference.Id, preference.InitPoint);
        }

        public async Task<MercadoPagoPaymentInfo?> GetPaymentAsync(long mercadoPagoPaymentId)
        {
            EnsureAccessToken();

            var client = new PaymentClient();
            try
            {
                var payment = await client.GetAsync(mercadoPagoPaymentId);
                return new MercadoPagoPaymentInfo
                {
                    Id = payment.Id ?? mercadoPagoPaymentId,
                    Status = payment.Status ?? string.Empty,
                    StatusDetail = payment.StatusDetail,
                    ExternalReference = payment.ExternalReference,
                    TransactionAmount = payment.TransactionAmount
                };
            }
            catch (MercadoPagoApiException ex) when (ex.StatusCode == 404)
            {
                return null;
            }
        }
    }
}
