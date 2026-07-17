using Jemar.Domain.Entities;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IShipmentRepository : IBaseRepository<Shipment>
    {
        Task<List<Shipment>> GetByClientIdAsync(Guid clientId);
        Task<ShipmentType?> GetShipmentTypeByIdAsync(int id);
        Task<PackageSize?> GetPackageSizeByIdAsync(int id);
        Task<List<ShipmentType>> GetShipmentTypesAsync();
        Task<List<PackageSize>> GetPackageSizesAsync();

    }
}