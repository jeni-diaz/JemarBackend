using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class ShipmentType
    {
        public int Id { get; set; }
        public ShipmentTypeEnum Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
