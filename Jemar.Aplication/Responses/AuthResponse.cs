namespace Jemar.Aplication.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        // Cuando es true, el login fue correcto pero falta verificar el
        // código de doble factor enviado por email (Token viene vacío).
        public bool RequiresTwoFactor { get; set; }
    }
}
