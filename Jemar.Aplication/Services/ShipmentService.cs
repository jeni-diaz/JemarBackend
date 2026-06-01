using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Interfaces;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;

    public ShipmentService(IShipmentRepository ShipmentRepository)
    {
        _shipmentRepository = ShipmentRepository;
    }

    public async Task<ShipmentResponse> Create(CreateShipmentRequest request)
    {
        var shipment = ShipmentMapper.ToEntity(request);

        await _shipmentRepository.AddAsync(shipment);
        await _shipmentRepository.SaveChangesAsync();

        return ShipmentMapper.ToResponse(shipment);
    }

    public async Task<List<ShipmentResponse>> GetAll()
    {
        var shipments = await _shipmentRepository.GetAllAsync();
        return ShipmentMapper.ToListResponse(shipments);
    }

    public async Task<ShipmentResponse?> GetById(Guid id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);

        if (shipment == null)
            return null;

        return ShipmentMapper.ToResponse(shipment);
    }

    public async Task<ShipmentResponse> Update(Guid id, UpdateShipmentRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);

        if (shipment is null)                                         
            throw new KeyNotFoundException($"Shipment {id} not found.");

        await _shipmentRepository.UpdateAsync(shipment);
        await _shipmentRepository.SaveChangesAsync();

        return ShipmentMapper.ToResponse(shipment);
    }


    public async Task<bool> Delete(Guid id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);

        if (shipment == null)
            return false;

        await _shipmentRepository.DeleteAsync(shipment.Id);
        await _shipmentRepository.SaveChangesAsync();
        return true;
    }

}