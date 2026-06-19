using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class ShipmentMapper
    {
        public static Shipment ToShipment(this CreateShipmentRequest request, Guid clientId)
        {
            return new Shipment
            {
                Id = Guid.NewGuid(),
                Origin = request.Origin,
                Destination = request.Destination,
                ShipmentTypeId = request.ShipmentTypeId,
                ShipmentStatusId = (int)ShipmentStatusEnum.Pending,
                ClientId = clientId,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
            };
        }

        public static ShipmentResponse ToShipmentResponse(this Shipment shipment)
        {
            return new ShipmentResponse
            {
                Id = shipment.Id,
                Origin = shipment.Origin,
                Destination = shipment.Destination,
                Price = shipment.Price,
                ShipmentType = shipment.ShipmentType?.Name.ToString() ?? string.Empty,
                ShipmentStatus = shipment.ShipmentStatus?.Name.ToString() ?? string.Empty
            };
        }

        public static List<ShipmentResponse> ToShipmentResponseList(this List<Shipment> shipments)
        {
            return shipments.Select(s => s.ToShipmentResponse()).ToList();
        }
    }
}