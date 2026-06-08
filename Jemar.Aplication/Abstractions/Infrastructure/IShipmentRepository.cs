using Jemar.Aplication.Abstractions.Infrastructure; // Importa la definición de IBaseRepository<T>
using Jemar.Domain.Entities; // Importa las entidades del dominio, incluyendo Shipment

namespace Jemar.Domain.Interfaces
{
    public interface IShipmentRepository : IBaseRepository<Shipment> // Define un contrato para trabajar con la entidad Shipment.
    {
        Task<List<Shipment>> GetByClientIdAsync(Guid clientId); // Método asíncrono que recibe el ID de un cliente  y devuelve una lista de envíos (Shipment) asociados a ese cliente.

    }
}