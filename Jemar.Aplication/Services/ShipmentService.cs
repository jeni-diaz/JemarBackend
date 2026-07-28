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
                var inlineImages = new Dictionary<string, byte[]>
                {
                    ["logo"] = LogoImage.Value,
                    ["robot"] = RobotImage.Value
                };
                await _emailService.SendAsync(recipient.Email, subject, body, inlineImages);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"No se pudo enviar el correo de confirmación del envío {shipment.Id}: {ex.Message}");
            }
        }

        // Assets del correo (logo + mascota) embebidos en el assembly. Se cargan
        // una sola vez y se incrustan por CID (cid:logo / cid:robot).
        private static readonly Lazy<byte[]> LogoImage = new(() => LoadEmailAsset("logo.png"));
        private static readonly Lazy<byte[]> RobotImage = new(() => LoadEmailAsset("robot.png"));

        private static byte[] LoadEmailAsset(string fileName)
        {
            var assembly = typeof(ShipmentService).Assembly;
            var resourceName = $"Jemar.Aplication.EmailAssets.{fileName}";
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"No se encontró el recurso embebido '{resourceName}'.");
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }

        private static string BuildShipmentEmail(string firstName, Shipment shipment)
        {
            // Paleta de la marca (misma que usa el front).
            const string gold = "#B07F11";
            const string dark = "#000000";
            const string green = "#3f7f07";
            const string red = "#b01b17";

            var type = shipment.ShipmentType?.Name.ToSpanish() ?? string.Empty;
            var size = shipment.PackageSize?.Name.ToSpanish() ?? string.Empty;
            var status = shipment.ShipmentStatus?.Name.ToSpanish() ?? string.Empty;
            var price = shipment.Price.ToString("N2", new System.Globalization.CultureInfo("es-AR"));

            // Color del estado igual que en la tabla del front.
            var statusColor = shipment.ShipmentStatus?.Name switch
            {
                ShipmentStatusEnum.Delivered => green,
                ShipmentStatusEnum.Cancelled => red,
                _ => gold
            };

            // Fila de detalle reutilizable (label en negrita + valor).
            string Row(string label, string value, string valueColor = "#222222") => $@"
                <tr>
                    <td style=""padding:10px 16px;border-bottom:1px solid #eeeeee;font-weight:bold;color:{dark};white-space:nowrap"">{label}</td>
                    <td style=""padding:10px 16px;border-bottom:1px solid #eeeeee;color:{valueColor}"">{value}</td>
                </tr>";

            return $@"
<div style=""margin:0;padding:24px 12px;background-color:#f4f4f4;font-family:Arial,Helvetica,sans-serif"">
  <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" align=""center"" style=""max-width:600px;margin:0 auto;background-color:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e0e0e0"">
    <tr>
      <td style=""background-color:{dark};padding:24px;text-align:center"">
        <img src=""cid:logo"" width=""130"" alt=""Jemar Envíos"" style=""display:inline-block;border:0"" />
      </td>
    </tr>
    <tr>
      <td style=""padding:28px 24px 8px;text-align:center"">
        <img src=""cid:robot"" width=""110"" alt="""" style=""display:inline-block;border:0"" />
        <h1 style=""margin:12px 0 4px;color:{green};font-size:24px"">¡Tu envío fue confirmado!</h1>
        <p style=""margin:0;color:#555555;font-size:15px"">Hola {firstName}, guardá este número para futuras consultas:</p>
      </td>
    </tr>
    <tr>
      <td style=""padding:16px 24px 4px;text-align:center"">
        <div style=""display:inline-block;border:2px solid {gold};border-radius:10px;padding:10px 18px"">
          <span style=""display:block;color:{gold};font-weight:bold;font-size:13px;letter-spacing:1px"">ENVÍO N°</span>
          <span style=""display:block;color:{dark};font-family:'Courier New',monospace;font-size:16px;word-break:break-all"">{shipment.Id}</span>
        </div>
      </td>
    </tr>
    <tr>
      <td style=""padding:20px 24px 4px"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;font-size:15px"">
          {Row("Tipo de envío", type)}
          {Row("Tamaño del paquete", size)}
          {Row("Origen", shipment.Origin)}
          {Row("Destino", shipment.Destination)}
          {Row("Distancia", $"{shipment.DistanceKm} km")}
          {Row("Estado", $"<strong>{status}</strong>", statusColor)}
        </table>
      </td>
    </tr>
    <tr>
      <td style=""padding:12px 24px 28px;text-align:center"">
        <span style=""display:block;color:{gold};font-weight:bold;font-size:14px;letter-spacing:1px"">PRECIO</span>
        <span style=""display:block;color:{gold};font-weight:bold;font-size:34px;line-height:1.2"">${price}</span>
      </td>
    </tr>
    <tr>
      <td style=""background-color:{dark};padding:16px;text-align:center;color:{gold};font-size:14px;font-weight:bold;letter-spacing:1px"">
        — Jemar Envíos
      </td>
    </tr>
  </table>
</div>";
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                throw new NotFoundException("Envío no encontrado.");

            int currentStatus = shipment.ShipmentStatusId;
            int nextStatus = request.ShipmentStatusId;

            if (currentUserRole == UserRoleEnum.Client.ToString())
            {
                // El cliente solo puede cancelar su propio envío, y solo mientras
                // sigue pendiente (todavía no salió a transitar).
                bool isOwner = shipment.CreatedByUserId == currentUserId ||
                               shipment.OnBehalfOfClientId == currentUserId;
                if (!isOwner)
                    throw new UnauthorizedAccessException("No tiene autorización para modificar este envío.");

                if (currentStatus != (int)ShipmentStatusEnum.Pending || nextStatus != (int)ShipmentStatusEnum.Cancelled)
                    throw new ValidationException("Los clientes solo pueden cancelar envíos que todavía están pendientes.");
            }
            else
            {
                // Empleado/SuperAdmin: máquina de estados completa, sobre cualquier envío.
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