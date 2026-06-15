using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IOpenStreetMapService _openStreetMapService;

        public ShipmentService(IShipmentRepository shipmentRepository, IOpenStreetMapService openStreetMapService)
        {
            _shipmentRepository = shipmentRepository;
            _openStreetMapService = openStreetMapService;
        }

        public async Task<List<ShipmentResponse>> GetAllAsync(Guid currentUserId, string currentUserRole)
        {
            List<Shipment> shipments;
            if (currentUserRole == UserRole.Client.ToString())
            {
                shipments = await _shipmentRepository.GetByClientIdAsync(currentUserId);
            }
            else
            {
                shipments = await _shipmentRepository.GetAllAsync();
            }

            return shipments.ToShipmentResponseList();
        }

        public async Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return null;

            if (currentUserRole == UserRole.Client.ToString() && shipment.ClientId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this shipment.");
            }

            return shipment.ToShipmentResponse();
        }

        public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid clientId)
        {
            // Validamos la dirección de Origen con tu servicio
            var sugerenciasOrigen = await _openStreetMapService.AutocompletarDireccionAsync(request.Origin);
            if (sugerenciasOrigen == null || !sugerenciasOrigen.Any())
            {
                throw new ArgumentException($"La dirección de origen '{request.Origin}' no es válida en Argentina.");
            }

            // Validamos la dirección de Destino con tu servicio
            var sugerenciasDestino = await _openStreetMapService.AutocompletarDireccionAsync(request.Destination);
            if (sugerenciasDestino == null || !sugerenciasDestino.Any())
            {
                throw new ArgumentException($"La dirección de destino '{request.Destination}' no es válida en Argentina.");
            }

            var shipment = request.ToShipment(clientId);

            // Reemplazamos por la dirección oficial/normalizada devuelta por OpenStreetMap
            shipment.Origin = sugerenciasOrigen.First();
            shipment.Destination = sugerenciasDestino.First();

            if (request.ShipmentTypeId == (int)ShipmentTypeEnum.Express)
            {
                shipment.Price = 50000m;
            }
            else if (request.ShipmentTypeId == (int)ShipmentTypeEnum.Standar)
            {
                shipment.Price = 30000m;
            }
            else
            {
                throw new ArgumentException("Invalid Shipment Type ID.");
            }

            var saved = await _shipmentRepository.AddAsync(shipment);
            return saved.ToShipmentResponse();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRole.Client.ToString())
            {
                throw new UnauthorizedAccessException("Clients are not authorized to update shipment status.");
            }

            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return false;

            int currentStatus = shipment.ShipmentStatusId;
            int nextStatus = request.ShipmentStatusId;


            if (currentStatus == (int)ShipmentStatusEnum.Pending)
            {
                if (nextStatus != (int)ShipmentStatusEnum.In_transit && nextStatus != (int)ShipmentStatusEnum.Cancelled)
                {
                    throw new ArgumentException("Pending shipment can only transition to In Transit or Cancelled.");
                }
            }
            else if (currentStatus == (int)ShipmentStatusEnum.In_transit)
            {
                if (nextStatus != (int)ShipmentStatusEnum.Delivered && nextStatus != (int)ShipmentStatusEnum.Cancelled)
                {
                    throw new ArgumentException("In Transit shipment can only transition to Delivered or Cancelled.");
                }
            }
            else if (currentStatus == (int)ShipmentStatusEnum.Delivered)
            {
                throw new ArgumentException("Delivered shipment status cannot be modified.");
            }
            else if (currentStatus == (int)ShipmentStatusEnum.Cancelled)
            {
                throw new ArgumentException("Cancelled shipment status cannot be modified.");
            }

            shipment.ShipmentStatusId = nextStatus;
            shipment.UpdatedAt = DateTime.UtcNow;


            shipment.EmployeeId = currentUserId;

            await _shipmentRepository.UpdateAsync(shipment);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return false;

            if (currentUserRole == UserRole.Client.ToString())
            {
                if (shipment.ClientId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this shipment.");
                }

                if (shipment.ShipmentStatusId != (int)ShipmentStatusEnum.Pending)
                {
                    throw new ArgumentException("Clients can only delete shipments that are still Pending.");
                }
            }

            await _shipmentRepository.DeleteAsync(id);
            return true;
        }
    }
}