
using System.Net.Http.Json;
using System.Web;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Services
{
    public class OpenStreetMapService : IOpenStreetMapService
    {
        private readonly HttpClient _httpClient;

        // Inyectamos el HttpClient mediante el constructor
        public OpenStreetMapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> AutocompletarDireccionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            // Codificamos el texto para que sea seguro en la URL
            var queryCodificado = HttpUtility.UrlEncode(query);

            // Llamada filtrada a Argentina (countrycodes=ar) con límite de 5 sugerencias
            var url = $"/search?format=json&limit=5&countrycodes=ar&q={queryCodificado}";

            // Realizamos la llamada HTTP
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Deserializamos el JSON de respuesta de OpenStreetMap
            var data = await response.Content.ReadFromJsonAsync<List<OpenStreetMapResponse>>();

            var sugerencias = new List<string>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    sugerencias.Add(item.DisplayName);
                }
            }

            return sugerencias;
        }
    }
}