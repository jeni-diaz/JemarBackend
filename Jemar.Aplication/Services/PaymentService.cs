using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IShipmentService _shipmentService;
        private readonly IMercadoPagoService _mercadoPagoService;

        public PaymentService(IPaymentRepository paymentRepository, IShipmentService shipmentService, IMercadoPagoService mercadoPagoService)
        {
            _paymentRepository = paymentRepository;
            _shipmentService = shipmentService;
            _mercadoPagoService = mercadoPagoService;
        }

        public async Task<CreatePaymentPreferenceResponse> CreateCheckoutAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole, string frontendBaseUrl, string backendBaseUrl)
        {
            // Recomputes the price server-side (never trust a client-supplied amount) and
            // validates the request the same way a normal shipment creation would.
            var quote = await _shipmentService.QuoteAsync(request, currentUserId, currentUserRole);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = quote.Price,
                PaymentStatusId = (int)PaymentStatusEnum.Pending,
                CreatedByUserId = currentUserId,
                CreatedByRole = currentUserRole,
                PendingShipmentId = request.Id ?? Guid.NewGuid(),
                PendingOrigin = request.Origin,
                PendingDestination = request.Destination,
                PendingShipmentTypeId = request.ShipmentTypeId,
                PendingPackageSizeId = request.PackageSizeId,
                PendingOnBehalfOfClientId = request.OnBehalfOfClientId,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };

            var (preferenceId, initPoint) = await _mercadoPagoService.CreatePreferenceAsync(
                payment.Id, quote.Price, $"Envío Jemar #{payment.PendingShipmentId}", frontendBaseUrl, backendBaseUrl);

            payment.PreferenceId = preferenceId;
            await _paymentRepository.AddAsync(payment);

            return new CreatePaymentPreferenceResponse
            {
                PreferenceId = preferenceId,
                InitPoint = initPoint
            };
        }

        private static int MapMercadoPagoStatus(string mercadoPagoStatus)
        {
            return mercadoPagoStatus switch
            {
                "approved" => (int)PaymentStatusEnum.Approved,
                "rejected" => (int)PaymentStatusEnum.Rejected,
                "cancelled" => (int)PaymentStatusEnum.Cancelled,
                "refunded" => (int)PaymentStatusEnum.Cancelled,
                "charged_back" => (int)PaymentStatusEnum.Cancelled,
                _ => (int)PaymentStatusEnum.Pending
            };
        }

        public async Task<PaymentStatusResponse?> SyncFromMercadoPagoAsync(long mercadoPagoPaymentId, Guid? currentUserId = null, string? currentUserRole = null)
        {
            var info = await _mercadoPagoService.GetPaymentAsync(mercadoPagoPaymentId);
            if (info == null)
                return null;

            if (!Guid.TryParse(info.ExternalReference, out var paymentId))
                return null;

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return null;

            if (currentUserId.HasValue && currentUserRole != null &&
                currentUserRole == UserRoleEnum.Client.ToString() &&
                payment.CreatedByUserId != currentUserId.Value)
            {
                throw new UnauthorizedAccessException("No tiene autorización para consultar este pago.");
            }

            payment.MercadoPagoPaymentId = info.Id.ToString();
            payment.StatusDetail = info.StatusDetail;
            var newStatus = MapMercadoPagoStatus(info.Status);
            payment.PaymentStatusId = newStatus;

            if (newStatus == (int)PaymentStatusEnum.Approved && payment.ShipmentId == null)
            {
                var createRequest = new CreateShipmentRequest
                {
                    Id = payment.PendingShipmentId,
                    Origin = payment.PendingOrigin,
                    Destination = payment.PendingDestination,
                    ShipmentTypeId = payment.PendingShipmentTypeId,
                    PackageSizeId = payment.PendingPackageSizeId,
                    OnBehalfOfClientId = payment.PendingOnBehalfOfClientId
                };

                var createdShipment = await _shipmentService.CreateAsync(createRequest, payment.CreatedByUserId, payment.CreatedByRole);
                payment.ShipmentId = createdShipment.Id;
            }

            await _paymentRepository.UpdateAsync(payment);

            var refreshed = await _paymentRepository.GetByIdAsync(payment.Id);
            return refreshed?.ToPaymentStatusResponse();
        }
    }
}
