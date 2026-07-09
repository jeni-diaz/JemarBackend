
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
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var encodedQuery = HttpUtility.UrlEncode(query);

            var url = $"/search?format=json&limit=1&countrycodes=ar&q={encodedQuery}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<List<OpenStreetMapResponse>>();

            var match = data?.FirstOrDefault();
            if (match == null)
                return null;

            if (!double.TryParse(match.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                !double.TryParse(match.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                return null;

            return new GeocodeResult
            {
                DisplayName = match.DisplayName,
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }
}