namespace Jemar.Aplication.Requests
{
    public class SignInRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Token de dispositivo guardado en el navegador de un login anterior
        // ya verificado por 2FA. Si es válido y no venció, se salta el paso
        // del código por email.
        public string? DeviceToken { get; set; }
    }
}
