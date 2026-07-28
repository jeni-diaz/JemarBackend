using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class ShipmentMapper
    {
        public static Shipment ToShipment(this CreateShipmentRequest request, Guid createdByUserId, int createdByRoleId)
        {
            return new Shipment
            {
                Id = request.Id ?? Guid.NewGuid(),
                Origin = request.Origin,
                Destination = request.Destination,
                ShipmentTypeId = request.ShipmentTypeId,
                ShipmentStatusId = (int)ShipmentStatusEnum.Pending,
                PackageSizeId = request.PackageSizeId,
                CreatedByUserId = createdByUserId,
                CreatedByRoleId = createdByRoleId,
                OnBehalfOfClientId = request.OnBehalfOfClientId,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
            };
        }

        public static ShipmentResponse ToShipmentResponse(this Shipment shipment)
        {
            // Dueño del envío: el cliente a nombre de quien se creó, o el creador.
            var client = shipment.OnBehalfOfClient ?? shipment.CreatedByUser;

            return new ShipmentResponse
            {
                Id = shipment.Id,
                Origin = shipment.Origin,
                Destination = shipment.Destination,
                DistanceKm = shipment.DistanceKm,
                Price = shipment.Price,
                ShipmentType = shipment.ShipmentType?.Name.ToString() ?? string.Empty,
                ShipmentStatus = shipment.ShipmentStatus?.Name.ToString() ?? string.Empty,
                PackageSize = shipment.PackageSize?.Name.ToString() ?? string.Empty,
                ClientName = client != null ? $"{client.FirstName} {client.LastName}".Trim() : string.Empty,
                ClientEmail = client?.Email ?? string.Empty,
                CreatedByUserId = shipment.CreatedByUserId,
                CreatedByRoleId = shipment.CreatedByRoleId,
                OnBehalfOfClientId = shipment.OnBehalfOfClientId,
                CreatedDateTime = shipment.CreatedDateTime,
                UpdatedDateTime = shipment.UpdatedDateTime
            };
        }

        public static List<ShipmentResponse> ToShipmentResponseList(this List<Shipment> shipments)
        {
            return shipments.Select(s => s.ToShipmentResponse()).ToList();
        }

        public static ShipmentTypeResponse ToShipmentTypeResponse(this ShipmentType shipmentType)
        {
            return new ShipmentTypeResponse
            {
                Id = shipmentType.Id,
                Name = shipmentType.Name.ToString(),
                Description = shipmentType.Description
            };
        }

        public static PackageSizeResponse ToPackageSizeResponse(this PackageSize packageSize)
        {
            return new PackageSizeResponse
            {
                Id = packageSize.Id,
                Name = packageSize.Name.ToString(),
                MaxLengthCm = packageSize.MaxLengthCm,
                MaxWidthCm = packageSize.MaxWidthCm,
                MaxHeightCm = packageSize.MaxHeightCm,
                BasePrice = packageSize.BasePrice,
                RatePerKm = packageSize.RatePerKm,
                Surcharge = packageSize.Surcharge
            };
        }
    }
}