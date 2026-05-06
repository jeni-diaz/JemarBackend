using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _ShipmentRepository;

    public ShipmentService(IShipmentRepository ShipmentRepository)
    {
        _ShipmentRepository = ShipmentRepository;
    }

    public async Task<List<ShipmentResponse>> GetAll()
    {
        var shipments = await _ShipmentRepository.GetAllAsync();
        return ShipmentMapper.ToListResponse(shipments);
    }

    public async Task<ShipmentResponse?> GetById(Guid id)
    {
        var shipment = await _ShipmentRepository.GetByIdAsync(id);

        if (shipment == null)
            return null;

        return ShipmentMapper.ToResponse(shipment);
    }

    public async Task<ShipmentResponse> Create(CreateShipmentRequest request)
    {
        var shipment = ShipmentMapper.ToEntity(request);

        await _ShipmentRepository.AddAsync(shipment);

        return ShipmentMapper.ToResponse(shipment);
    }

    public async Task<ShipmentResponse> Update(Guid id, UpdateShipmentRequest request)
    {
        var shipment = await _ShipmentRepository.GetByIdAsync(id);
        
        await _ShipmentRepository.UpdateAsync(shipment);

        return ShipmentMapper.ToResponse(shipment);
    }

}