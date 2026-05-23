using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;

public static class ShipmentMapper
{
    public static Shipment ToEntity(CreateShipmentRequest request)
    {
        return new Shipment
        {
            Id = Guid.NewGuid(),
            Origin = request.Origin,
            Destination = request.Destination,
            Price = request.Price,
            ShipmentTypeId = request.ShipmentTypeId,
            ShipmentStatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static ShipmentResponse ToResponse(Shipment shipment)
    {
        return new ShipmentResponse
        {
            Id = shipment.Id,
            Origin = shipment.Origin,
            Destination = shipment.Destination,
            Price = shipment.Price,
            ShipmentType = shipment.ShipmentType.Name.ToString(),
            ShipmentStatus = shipment.ShipmentStatus.Name.ToString()
        };
    }

    public static List<ShipmentResponse> ToListResponse(List<Shipment> shipments)
    {
        return shipments.Select(ToResponse).ToList();
    }
}