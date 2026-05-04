using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _repo;

    public ShipmentService(IShipmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ShipmentResponse>> GetAll()
    {
        var shipments = await _repo.GetAllAsync();
        return ShipmentMapper.ToListResponse(shipments);
    }

    public async Task<ShipmentResponse?> GetById(Guid id)
    {
        var shipment = await _repo.GetByIdAsync(id);

        if (shipment == null)
            return null;

        return ShipmentMapper.ToResponse(shipment);
    }

    public async Task<ShipmentResponse> Create(CreateShipmentRequest request)
    {
        var shipment = ShipmentMapper.ToEntity(request);

        await _repo.AddAsync(shipment);

        return ShipmentMapper.ToResponse(shipment);
    }
}