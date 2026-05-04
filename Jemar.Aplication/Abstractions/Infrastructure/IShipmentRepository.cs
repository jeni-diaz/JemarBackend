using Jemar.Domain.Entities;

public interface IShipmentRepository
{
    Task<List<Shipment>> GetAllAsync();
    Task<Shipment?> GetByIdAsync(Guid id);
    Task AddAsync(Shipment shipment);
}
