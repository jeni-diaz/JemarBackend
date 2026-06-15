using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class ShipmentType : BaseEntity
    {
        public ShipmentTypeEnum Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
