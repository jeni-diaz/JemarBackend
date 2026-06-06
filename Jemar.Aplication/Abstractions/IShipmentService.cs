using Jemar.Aplication.Requests; // Importa los DTOs de entrada (requests) que recibe el servicio
using Jemar.Aplication.Responses; // Importa los DTOs de salida (responses) que devuelve el servicio

namespace Jemar.Aplication.Abstractions
{
    public interface IShipmentService // Define el contrato que debe implementar cualquier servicio de Shipments
    {
        Task<List<ShipmentResponse>>GetAllAsync(Guid currentUserId, string currentUserRole); // Obtiene todos los envíos, currentUserId y currentUserRole se usan para aplicar permisos.
        Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole);// Obtiene un envío por su ID. Devuelve null si no existe o no puede accederse.
        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid clientId); // Crea un nuevo envío. request contiene los datos del envío. clientId identifica al cliente que realiza la operación.
        Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole); // Actualiza el estado de un envío existente. Devuelve true si la actualización fue exitosa. Devuelve false si no se encontró o no tiene permisos.
        Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole); // Elimina un envío. Devuelve true si se eliminó correctamente. Devuelve false si no existe o no tiene permisos.
    }
}