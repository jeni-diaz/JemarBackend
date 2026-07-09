using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class PackageSize
    {
        public int Id { get; set; }
        public PackageSizeEnum Name { get; set; }
        public decimal MaxLengthCm { get; set; }
        public decimal MaxWidthCm { get; set; }
        public decimal MaxHeightCm { get; set; }
        public decimal BasePrice { get; set; }
        public decimal RatePerKm { get; set; }
        public decimal Surcharge { get; set; }
    }
}
