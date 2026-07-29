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

        // Cuando es true, la cuenta existe pero el email todavía no fue
        // verificado: enviamos un código y el frontend debe pedirlo (Token vacío).
        public bool RequiresEmailVerification { get; set; }

        // Se devuelve solo cuando se verificó un código con éxito: el
        // frontend debe guardarlo (localStorage) y reenviarlo en el próximo
        // login para no tener que verificar 2FA de nuevo desde ese navegador.
        public string? DeviceToken { get; set; }
    }
}
