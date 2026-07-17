using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IOpenStreetMapService
    {
        Task<GeocodeResult?> GeocodeAddressAsync(string query);
        Task<List<GeocodeResult>> SearchAddressesAsync(string query, int limit = 5);
    }
}
