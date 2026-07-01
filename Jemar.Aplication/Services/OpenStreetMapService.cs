
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

        public async Task<List<string>> AutocompleteAddressAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var encodedQuery = HttpUtility.UrlEncode(query);

            var url = $"/search?format=json&limit=5&countrycodes=ar&q={encodedQuery}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<List<OpenStreetMapResponse>>();

            var suggestions = new List<string>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    suggestions.Add(item.DisplayName);
                }
            }

            return suggestions;
        }
    }
}