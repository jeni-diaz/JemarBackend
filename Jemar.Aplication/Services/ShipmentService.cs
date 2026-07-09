using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Common;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Jemar.Aplication.Exceptions;

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
            if (currentUserRole == UserRoleEnum.Client.ToString())
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
                throw new NotFoundException("Envío no encontrado.");

            if (currentUserRole == UserRoleEnum.Client.ToString() &&
                shipment.CreatedByUserId != currentUserId &&
                shipment.OnBehalfOfClientId != currentUserId)
            {
                throw new UnauthorizedAccessException("No tiene autorización para ver este envío.");
            }

            return shipment.ToShipmentResponse();
        }

        public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            var origin = await _openStreetMapService.GeocodeAddressAsync(request.Origin);
            if (origin == null)
                throw new ValidationException($"La dirección de origen '{request.Origin}' no es válida en Argentina.");

            var destination = await _openStreetMapService.GeocodeAddressAsync(request.Destination);
            if (destination == null)
                throw new ValidationException($"La dirección de destino '{request.Destination}' no es válida en Argentina.");

            var shipmentType = await _shipmentRepository.GetShipmentTypeByIdAsync(request.ShipmentTypeId);
            if (shipmentType == null)
                throw new ValidationException("ID de tipo de envío no válido.");

            var packageSize = await _shipmentRepository.GetPackageSizeByIdAsync(request.PackageSizeId);
            if (packageSize == null)
                throw new ValidationException("ID de tamaño de paquete no válido.");

            var distanceKm = GeoCalculator.HaversineDistanceKm(
                origin.Latitude, origin.Longitude,
                destination.Latitude, destination.Longitude);
            var roundedDistanceKm = Math.Round((decimal)distanceKm, 2);

            int createdByRoleId = 0;
            if (currentUserRole == UserRoleEnum.SuperAdmin.ToString())
            {
                createdByRoleId = (int)UserRoleEnum.SuperAdmin;
            }
            else if (currentUserRole == UserRoleEnum.Employee.ToString())
            {
                createdByRoleId = (int)UserRoleEnum.Employee;
            }
            else
            {
                createdByRoleId = (int)UserRoleEnum.Client;
            }

            if (createdByRoleId == (int)UserRoleEnum.Client)
            {
                request.OnBehalfOfClientId = null;
            }
            else
            {
                if (request.OnBehalfOfClientId == null || request.OnBehalfOfClientId == Guid.Empty)
                {
                    throw new ValidationException("El empleado debe especificar para qué cliente se crea este envío.");
                }
            }

            var shipment = request.ToShipment(currentUserId, createdByRoleId);
            shipment.Origin = origin.DisplayName;
            shipment.Destination = destination.DisplayName;
            shipment.DistanceKm = roundedDistanceKm;
            shipment.Price = packageSize.BasePrice + (packageSize.RatePerKm * roundedDistanceKm) + packageSize.Surcharge;

            var saved = await _shipmentRepository.AddAsync(shipment);
            var fullyLoadedShipment = await _shipmentRepository.GetByIdAsync(saved.Id);
            return fullyLoadedShipment!.ToShipmentResponse();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            if (currentUserRole == UserRoleEnum.Client.ToString())
                throw new UnauthorizedAccessException("Los clientes no están autorizados a actualizar el estado del envío.");

            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                throw new NotFoundException("Envío no encontrado.");

            int currentStatus = shipment.ShipmentStatusId;
            int nextStatus = request.ShipmentStatusId;

            if (currentStatus == (int)ShipmentStatusEnum.Pending)
            {
                if (nextStatus != (int)ShipmentStatusEnum.InTransit && nextStatus != (int)ShipmentStatusEnum.Cancelled)
                    throw new ValidationException("Un envío pendiente solo puede pasar al estado En tránsito o Cancelado.");
            }
            else if (currentStatus == (int)ShipmentStatusEnum.InTransit)
            {
                if (nextStatus != (int)ShipmentStatusEnum.Delivered && nextStatus != (int)ShipmentStatusEnum.Cancelled)
                    throw new ValidationException("Un envío En tránsito solo puede pasar al estado de Entregado o Cancelado.");
            }
            else if (currentStatus == (int)ShipmentStatusEnum.Delivered)
            {
                throw new ValidationException("No se puede modificar el estado de un envío Entregado.");
            }
            else if (currentStatus == (int)ShipmentStatusEnum.Cancelled)
            {
                throw new ValidationException("El estado de un envío Cancelado no se puede modificar.");
            }

            shipment.ShipmentStatusId = nextStatus;
            shipment.UpdatedDateTime = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                throw new NotFoundException("Envío no encontrado.");

            if (currentUserRole == UserRoleEnum.Client.ToString())
            {
                if (shipment.CreatedByUserId != currentUserId &&
                    shipment.OnBehalfOfClientId != currentUserId)
                {
                    throw new UnauthorizedAccessException("No tiene autorización para eliminar este envío.");
                }

                if (shipment.ShipmentStatusId != (int)ShipmentStatusEnum.Pending)
                    throw new ValidationException("Los clientes solo pueden eliminar los envíos que aún están pendientes.");
            }

            await _shipmentRepository.DeleteAsync(id);
            return true;
        }
    }
}