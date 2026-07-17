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
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public ShipmentService(IShipmentRepository shipmentRepository, IOpenStreetMapService openStreetMapService, IUserRepository userRepository, IUserService userService, IEmailService emailService)
        {
            _shipmentRepository = shipmentRepository;
            _openStreetMapService = openStreetMapService;
            _userRepository = userRepository;
            _userService = userService;
            _emailService = emailService;
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

        // Arma el envío (geocodifica, valida y calcula el precio) SIN persistirlo.
        // Lo usan tanto la cotización (solo lo devuelve) como la creación (lo guarda).
        private async Task<Shipment> BuildShipmentAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole)
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

                var client = await _userRepository.GetByIdAsync(request.OnBehalfOfClientId.Value);
                if (client == null || client.RoleId != (int)UserRoleEnum.Client)
                {
                    throw new ValidationException("El cliente seleccionado no es válido.");
                }
            }

            var shipment = request.ToShipment(currentUserId, createdByRoleId);
            shipment.Origin = origin.DisplayName;
            shipment.Destination = destination.DisplayName;
            shipment.DistanceKm = roundedDistanceKm;
            shipment.Price = packageSize.BasePrice + (packageSize.RatePerKm * roundedDistanceKm) + packageSize.Surcharge;

            return shipment;
        }

        public async Task<ShipmentQuoteResponse> QuoteAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            var shipment = await BuildShipmentAsync(request, currentUserId, currentUserRole);
            return new ShipmentQuoteResponse
            {
                Id = shipment.Id,
                Price = shipment.Price,
                DistanceKm = shipment.DistanceKm
            };
        }

        public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            var shipment = await BuildShipmentAsync(request, currentUserId, currentUserRole);

            var saved = await _shipmentRepository.AddAsync(shipment);
            var fullyLoadedShipment = await _shipmentRepository.GetByIdAsync(saved.Id);

            await SendShipmentEmailSafeAsync(fullyLoadedShipment!, currentUserId);

            return fullyLoadedShipment!.ToShipmentResponse();
        }

        // Envía al cliente el detalle del envío (con el número para consultas futuras).
        // Si el correo falla, se registra pero NO se interrumpe la creación: el envío
        // ya quedó guardado.
        private async Task SendShipmentEmailSafeAsync(Shipment shipment, Guid currentUserId)
        {
            try
            {
                var recipientId = shipment.OnBehalfOfClientId ?? currentUserId;
                var recipient = await _userRepository.GetByIdAsync(recipientId);
                if (recipient == null || string.IsNullOrWhiteSpace(recipient.Email))
                    return;

                var subject = $"Envío N° {shipment.Id} confirmado - Jemar Envíos";
                var body = BuildShipmentEmail(recipient.FirstName, shipment);
                await _emailService.SendAsync(recipient.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"No se pudo enviar el correo de confirmación del envío {shipment.Id}: {ex.Message}");
            }
        }

        private static string BuildShipmentEmail(string firstName, Shipment shipment)
        {
            var type = shipment.ShipmentType?.Name.ToString() ?? string.Empty;
            var size = shipment.PackageSize?.Name.ToString() ?? string.Empty;
            var status = shipment.ShipmentStatus?.Name.ToString() ?? string.Empty;
            var price = shipment.Price.ToString("N2", new System.Globalization.CultureInfo("es-AR"));

            return $@"<div style=""font-family:Arial,sans-serif;color:#222"">
                <h2>¡Tu envío fue confirmado!</h2>
                <p>Hola {firstName},</p>
                <p>Guardá este número de envío, lo vas a necesitar para futuras consultas:</p>
                <p style=""font-size:20px;font-weight:bold"">Envío N° {shipment.Id}</p>
                <table style=""border-collapse:collapse"" cellpadding=""6"">
                    <tr><td style=""font-weight:bold"">Tipo de envío</td><td>{type}</td></tr>
                    <tr><td style=""font-weight:bold"">Tamaño del paquete</td><td>{size}</td></tr>
                    <tr><td style=""font-weight:bold"">Origen</td><td>{shipment.Origin}</td></tr>
                    <tr><td style=""font-weight:bold"">Destino</td><td>{shipment.Destination}</td></tr>
                    <tr><td style=""font-weight:bold"">Distancia</td><td>{shipment.DistanceKm} km</td></tr>
                    <tr><td style=""font-weight:bold"">Precio</td><td>${price}</td></tr>
                    <tr><td style=""font-weight:bold"">Estado</td><td>{status}</td></tr>
                </table>
                <p>— Jemar Envíos</p>
            </div>";
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

        public async Task<List<ShipmentTypeResponse>> GetShipmentTypesAsync()
        {
            var types = await _shipmentRepository.GetShipmentTypesAsync();
            return types.Select(t => t.ToShipmentTypeResponse()).ToList();
        }

        public async Task<List<PackageSizeResponse>> GetPackageSizesAsync()
        {
            var sizes = await _shipmentRepository.GetPackageSizesAsync();
            return sizes.Select(s => s.ToPackageSizeResponse()).ToList();
        }

        public async Task<List<GeocodeResult>> SearchAddressesAsync(string query)
        {
            return await _openStreetMapService.SearchAddressesAsync(query);
        }

        public async Task<List<UserResponse>> GetClientsAsync()
        {
            var clients = await _userRepository.GetByRoleAsync(UserRoleEnum.Client);
            return clients.ToUserResponseList();
        }

        public async Task<EmailAvailabilityResponse> CheckEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new EmailAvailabilityResponse { Exists = false };

            var user = await _userRepository.GetByEmailAsync(email.Trim());
            if (user == null)
                return new EmailAvailabilityResponse { Exists = false };

            var role = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString();
            return new EmailAvailabilityResponse { Exists = true, Role = role };
        }

        public async Task<UserResponse> CreateClientAsync(SignUpRequest request)
        {
            return await _userService.CreateClientAsync(request);
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