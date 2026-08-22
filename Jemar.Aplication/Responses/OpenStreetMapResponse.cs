using System.Text.Json.Serialization;

namespace Jemar.Aplication.Responses
{
    internal class OpenStreetMapResponse
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public string Lat { get; set; } = string.Empty;

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public OpenStreetMapAddress? Address { get; set; }
    }

    internal class OpenStreetMapAddress
    {
        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("house_number")]
        public string? HouseNumber { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("town")]
        public string? Town { get; set; }

        [JsonPropertyName("village")]
        public string? Village { get; set; }

        [JsonPropertyName("suburb")]
        public string? Suburb { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
