namespace Jemar.Aplication.Responses
{
    public class PackageSizeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MaxLengthCm { get; set; }
        public decimal MaxWidthCm { get; set; }
        public decimal MaxHeightCm { get; set; }
        public decimal BasePrice { get; set; }
        public decimal RatePerKm { get; set; }
        public decimal Surcharge { get; set; }
    }
}
