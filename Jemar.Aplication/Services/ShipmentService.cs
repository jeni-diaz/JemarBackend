using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;

public class ShipmentService
{
    private readonly IShipmentRepository _repo;

    public ShipmentService(IShipmentRepository repo)
    {
        _repo = repo;
    }

    public async Task CreateAsync(CreateShipmentRequest request)
    {
        var shipment = new Shipment
        {
            Origin = request.Origin,
            Destination = request.Destination,
            Price = request.Price,
            ShipmentTypeId = request.ShipmentTypeId,
            ShipmentStatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(shipment);
    }

    public async Task<List<ShipmentResponse>> GetAllAsync()
    {
        var shipments = await _repo.GetAllAsync();

        return shipments.Select(x => new ShipmentResponse
        {
            Origin = x.Origin,
            Destination = x.Destination,
            Price = x.Price,
            ShipmentType = x.ShipmentType.Name.ToString(),
            ShipmentStatus = x.ShipmentStatus.Name.ToString()
        }).ToList();
    }
}