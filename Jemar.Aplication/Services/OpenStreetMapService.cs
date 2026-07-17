
using System.Globalization;
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

            var url = $"/search?format=json&limit={limit}&countrycodes=ar&q={encodedQuery}";

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
                    DisplayName = match.DisplayName,
                    Latitude = latitude,
                    Longitude = longitude
                });
            }

            return results;
        }
    }
}