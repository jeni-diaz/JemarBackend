using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IShipmentService
    {
        Task<List<ShipmentResponse>> GetAll();
        Task<ShipmentResponse?> GetById(Guid id);
        Task<ShipmentResponse> Create(CreateShipmentRequest request);
        Task<bool> Delete(Guid id);
    }
}