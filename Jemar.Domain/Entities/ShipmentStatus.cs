using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class ShipmentStatus
    {
        public int Id { get; set; }
        public ShipmentStatusEnum Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
