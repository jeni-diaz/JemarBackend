using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;
using Jemar.Domain.Interfaces;

namespace Jemar.Aplication.Services
{
    public class ShipmentService : IShipmentService
    {
        // Variable privada para guardar el repositorio de envíos que se conecta a la base de datos
        private readonly IShipmentRepository _shipmentRepository;

        // Constructor: Aquí recibimos el repositorio inyectado y lo guardamos en nuestra variable privada
        public ShipmentService(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        // Método para obtener la lista de todos los envíos
        public async Task<List<ShipmentResponse>> GetAllAsync(Guid currentUserId, string currentUserRole)
        {
            // Guardamos un verdadero/falso: será true si el rol del usuario actual es "Client"
            var isClient = currentUserRole == UserRole.Client.ToString();

            // Si es cliente, busca solo los envíos que le pertenecen a su ID. Si no es cliente, busca TODOS los envíos.
            var shipments = isClient
                ? await _shipmentRepository.GetByClientIdAsync(currentUserId)
                : await _shipmentRepository.GetAllAsync();

            // Convertimos la lista de envíos de la base de datos a una lista de respuestas (DTO) para el frontend
            return shipments.ToShipmentListResponse();
        }

        // Método para buscar un solo envío por su ID único
        public async Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            // Vamos a la base de datos a buscar el envío por su ID
            var shipment = await _shipmentRepository.GetByIdAsync(id);

            // Si no se encontró ningún envío en la base de datos, devolvemos null (vacío)
            if (shipment == null) return null;

            // Seguridad: Si el usuario es cliente, pero el envío que busca pertenece a OTRO cliente, bloqueamos el acceso
            if (currentUserRole == UserRole.Client.ToString() && shipment.ClientId != currentUserId)
            {
                // Disparamos un error de acceso no autorizado
                throw new UnauthorizedAccessException("You are not authorized to access this shipment.");
            }

            // Si todo está bien, convertimos el envío a formato de respuesta y lo devolvemos
            return shipment.ToShipmentResponse();
        }

        // Método para crear un nuevo envío en el sistema
        public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid clientId)
        {
            // Convertimos los datos que nos envió el usuario (request) en una entidad "Shipment" asignándole el dueño (clientId)
            var shipment = request.ToShipment(clientId);

            // Analizamos el ID del tipo de envío para cobrar la tarifa correspondiente
            shipment.Price = request.ShipmentTypeId switch
            {
                // Si eligió tipo Standard (común), el precio es 30000
                (int)ShipmentTypeEnum.Standard => 30000m,

                // Si eligió tipo Express (rápido), el precio es 50000
                (int)ShipmentTypeEnum.Express => 50000m,

                // Si mandó cualquier otro número inválido, rompemos el flujo con un error de argumento
                _ => throw new ArgumentException("Invalid shipment type ID.")
            };

            // Mandamos el envío listo con su precio al repositorio para que lo guarde en la base de datos
            await _shipmentRepository.AddAsync(shipment);

            // Convertimos el envío guardado a formato de respuesta para avisarle al usuario que se creó con éxito
            return shipment.ToShipmentResponse();
        }

        // Método para actualizar el estado logístico del envío (ej: cambiar de Pendiente a En Tránsito)
        public async Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole)
        {
            // Seguridad: Si el usuario es un cliente, no tiene permiso de cambiar estados. Solo los empleados pueden.
            if (currentUserRole == UserRole.Client.ToString())
            {
                throw new UnauthorizedAccessException("Clients are not authorized to update shipment status.");
            }

            // Buscamos el envío que se quiere actualizar en la base de datos
            var shipment = await _shipmentRepository.GetByIdAsync(id);

            // Si el envío no existe en el sistema, devolvemos false (operación fallida)
            if (shipment == null) return false;

            // Convertimos los números de estado a palabras legibles (Enums) para poder compararlos de forma segura
            var current = (ShipmentStatusEnum)shipment.ShipmentStatusId;
            var next = (ShipmentStatusEnum)request.ShipmentStatusId;

            // Evaluamos la combinación (Estado Actual, Estado Siguiente) para validar si el cambio es legal en el negocio
            _ = (current, next) switch
            {
                // Combinaciones permitidas:
                (ShipmentStatusEnum.Pending, ShipmentStatusEnum.InTransit) => true,   // De Pendiente puede pasar a En Tránsito
                (ShipmentStatusEnum.Pending, ShipmentStatusEnum.Cancelled) => true,   // De Pendiente puede pasar a Cancelado
                (ShipmentStatusEnum.InTransit, ShipmentStatusEnum.Delivered) => true, // De En Tránsito puede pasar a Entregado
                (ShipmentStatusEnum.InTransit, ShipmentStatusEnum.Cancelled) => true, // De En Tránsito puede pasar a Cancelado

                // Bloqueos de seguridad para estados finales:
                (ShipmentStatusEnum.Delivered, _) => throw new ArgumentException("Delivered shipment status cannot be modified."), // Un paquete entregado no se puede volver a tocar
                (ShipmentStatusEnum.Cancelled, _) => throw new ArgumentException("Cancelled shipment status cannot be modified."), // Un paquete cancelado no se puede reabrir

                // Cualquier otra combinación no escrita arriba (ej: de Pendiente saltar directo a Entregado) dará error
                _ => throw new ArgumentException($"Invalid status transition from {current} to {next}.")
            };

            // Si la validación del switch fue exitosa, aplicamos el nuevo número de estado al envío
            shipment.ShipmentStatusId = (int)next;

            // Guardamos la fecha y hora exacta en la que se hizo esta modificación en formato UTC
            shipment.UpdatedAt = DateTime.UtcNow;

            // Registramos el ID del empleado que realizó el cambio de estado para auditoría
            shipment.EmployeeId = currentUserId;

            // Enviamos el objeto con los nuevos datos al repositorio para actualizar la base de datos
            await _shipmentRepository.UpdateAsync(shipment);

            // Retornamos true indicando que todo el proceso se completó correctamente
            return true;
        }

        // Método para eliminar un envío del sistema
        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            // Buscamos el envío en la base de datos para ver si existe
            var shipment = await _shipmentRepository.GetByIdAsync(id);

            // Si el envío no existe, devolvemos false inmediantamente
            if (shipment == null) return false;

            // Si el usuario que intenta borrar es un Cliente, aplicamos restricciones estrictas
            if (currentUserRole == UserRole.Client.ToString())
            {
                // Restricción 1: Un cliente no puede borrar el envío de otro cliente. Validamos que sea el dueño.
                if (shipment.ClientId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this shipment.");
                }

                // Restricción 2: El cliente solo puede borrar el envío si aún está "Pending" (Pendiente). Si ya viajó o se entregó, no puede.
                if (shipment.ShipmentStatusId != (int)ShipmentStatusEnum.Pending)
                {
                    throw new ArgumentException("Clients can only delete shipments that are still Pending.");
                }
            }

            // Si es un Empleado/Admin o si es el Cliente dueño con un paquete pendiente, procedemos a borrarlo físicamente de la BD
            await _shipmentRepository.DeleteAsync(id);

            // Retornamos true indicando que el envío fue eliminado con éxito
            return true;
        }
    }
}