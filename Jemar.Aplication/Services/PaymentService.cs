using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IMercadoPagoService _mercadoPagoService;

        public PaymentService(IPaymentRepository paymentRepository, IShipmentRepository shipmentRepository, IMercadoPagoService mercadoPagoService)
        {
            _paymentRepository = paymentRepository;
            _shipmentRepository = shipmentRepository;
            _mercadoPagoService = mercadoPagoService;
        }

        private static void EnsureOwnership(Shipment shipment, Guid currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoleEnum.Client.ToString() &&
                shipment.CreatedByUserId != currentUserId &&
                shipment.OnBehalfOfClientId != currentUserId)
            {
                throw new UnauthorizedAccessException("No tiene autorización para pagar este envío.");
            }
        }

        public async Task<CreatePaymentPreferenceResponse> CreatePreferenceAsync(Guid shipmentId, Guid currentUserId, string currentUserRole, string frontendBaseUrl, string backendBaseUrl)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
            if (shipment == null)
                throw new NotFoundException("Envío no encontrado.");

            EnsureOwnership(shipment, currentUserId, currentUserRole);

            if (shipment.Price <= 0)
                throw new ArgumentException("El envío no tiene un precio válido para pagar.");

            var existingPayments = await _paymentRepository.GetByShipmentIdAsync(shipmentId);
            if (existingPayments.Any(p => p.PaymentStatusId == (int)PaymentStatusEnum.Approved))
                throw new ConflictException("Este envío ya fue pagado.");

            var (preferenceId, initPoint) = await _mercadoPagoService.CreatePreferenceAsync(shipment, frontendBaseUrl, backendBaseUrl);

            await _paymentRepository.AddAsync(new Payment
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id,
                Amount = shipment.Price,
                PaymentStatusId = (int)PaymentStatusEnum.Pending,
                PreferenceId = preferenceId,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            });

            return new CreatePaymentPreferenceResponse
            {
                PreferenceId = preferenceId,
                InitPoint = initPoint
            };
        }

        public async Task<PaymentStatusResponse> GetStatusAsync(Guid shipmentId, Guid currentUserId, string currentUserRole)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
            if (shipment == null)
                throw new NotFoundException("Envío no encontrado.");

            EnsureOwnership(shipment, currentUserId, currentUserRole);

            var payment = await _paymentRepository.GetLatestByShipmentIdAsync(shipmentId);
            if (payment == null)
            {
                return new PaymentStatusResponse
                {
                    ShipmentId = shipmentId,
                    Status = "None",
                    Amount = shipment.Price
                };
            }

            return payment.ToPaymentStatusResponse();
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

            if (!Guid.TryParse(info.ExternalReference, out var shipmentId))
                return null;

            var payment = await _paymentRepository.GetPendingByShipmentIdAsync(shipmentId)
                ?? await _paymentRepository.GetLatestByShipmentIdAsync(shipmentId);

            if (payment == null)
            {
                var shipmentForNewPayment = await _shipmentRepository.GetByIdAsync(shipmentId);
                if (shipmentForNewPayment == null)
                    return null;

                payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = shipmentId,
                    Amount = info.TransactionAmount ?? shipmentForNewPayment.Price,
                    PaymentStatusId = (int)PaymentStatusEnum.Pending,
                    CreatedDateTime = DateTime.UtcNow,
                    UpdatedDateTime = DateTime.UtcNow
                };
                await _paymentRepository.AddAsync(payment);
            }

            if (currentUserId.HasValue && currentUserRole != null)
            {
                var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
                if (shipment == null)
                    throw new NotFoundException("Envío no encontrado.");

                EnsureOwnership(shipment, currentUserId.Value, currentUserRole);
            }

            payment.MercadoPagoPaymentId = info.Id.ToString();
            payment.StatusDetail = info.StatusDetail;
            payment.PaymentStatusId = MapMercadoPagoStatus(info.Status);

            await _paymentRepository.UpdateAsync(payment);

            var refreshed = await _paymentRepository.GetByIdAsync(payment.Id);
            return refreshed?.ToPaymentStatusResponse();
        }
    }
}
