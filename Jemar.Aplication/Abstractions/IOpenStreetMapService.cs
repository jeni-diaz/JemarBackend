using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IOpenStreetMapService
    {
        Task<GeocodeResult?> GeocodeAddressAsync(string query);
    }
}
