using System.Text.Json.Serialization;

namespace Jemar.Aplication.Responses
{
    internal class OpenStreetMapResponse
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
    }
}
