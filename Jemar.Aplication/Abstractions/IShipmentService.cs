using Jemar.Aplication.Requests; // Importa los DTOs de entrada (requests) que recibe el servicio
using Jemar.Aplication.Responses; // Importa los DTOs de salida (responses) que devuelve el servicio

namespace Jemar.Aplication.Abstractions
{
    // Una interfaz actúa como un plano o contrato: obliga a la clase que la implemente (ShipmentService) a escribir el código real de estos métodos
    public interface IShipmentService
    {
        // Contrato para obtener la lista de todos los envíos. Pide obligatoriamente el ID y el Rol del usuario para saber qué filtrar en el código real.
        Task<List<ShipmentResponse>> GetAllAsync(Guid currentUserId, string currentUserRole);

        // Contrato para obtener un envío por su ID. El signo "?" en "ShipmentResponse?" avisa que este método puede terminar devolviendo un objeto o un "null".
        Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole);

        // Contrato para crear un envío nuevo. Recibe el formulario del frontend (request) y el ID del cliente dueño. Devuelve los datos del envío ya creado.
        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid clientId);

        // Contrato para cambiar el estado logístico de un paquete. Devuelve true si el cambio fue exitoso, o false si no se encontró el envío.
        Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole);

        // Contrato para eliminar un envío del sistema. Devuelve true si se pudo borrar con éxito, o false si falló la operación.
        Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole);
    }
}