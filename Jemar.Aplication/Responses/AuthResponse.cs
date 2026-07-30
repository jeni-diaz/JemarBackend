using System.Text.Json.Serialization;

namespace Jemar.Aplication.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public bool RequiresTwoFactor { get; set; }

        public bool RequiresEmailVerification { get; set; }

        public string? DeviceToken { get; set; }

        [JsonIgnore]
        public string? RefreshTokenPlaintext { get; set; }
    }
}
