
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Web;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Services
{
    public class OpenStreetMapService : IOpenStreetMapService
    {
        private readonly HttpClient _httpClient;
        public OpenStreetMapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeocodeResult?> GeocodeAddressAsync(string query)
        {
            var results = await SearchAddressesAsync(query, 1);
            return results.FirstOrDefault();
        }

        public async Task<List<GeocodeResult>> SearchAddressesAsync(string query, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<GeocodeResult>();

            var encodedQuery = HttpUtility.UrlEncode(query);

            var url = $"/search?format=json&limit={limit}&countrycodes=ar&addressdetails=1&q={encodedQuery}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<List<OpenStreetMapResponse>>();
            if (data == null)
                return new List<GeocodeResult>();

            var results = new List<GeocodeResult>();
            foreach (var match in data)
            {
                if (!double.TryParse(match.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                    !double.TryParse(match.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                    continue;

                results.Add(new GeocodeResult
                {
                    DisplayName = FormatAddress(match),
                    Latitude = latitude,
                    Longitude = longitude
                });
            }

            return results;
        }

        private static string FormatAddress(OpenStreetMapResponse match)
        {
            var address = match.Address;
            if (address == null)
                return match.DisplayName;

            var city = address.City ?? address.Town ?? address.Village ?? address.Suburb;

            var parts = new[] { address.Road, address.HouseNumber, city, address.State }
                .Where(part => !string.IsNullOrWhiteSpace(part));

            var formatted = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(formatted) ? match.DisplayName : formatted;
        }
    }
}