namespace Jemar.Domain.Entities
{
    // Navegador ya verificado por 2FA para un usuario. Mientras exista un
    // registro vigente (no vencido) para el token que manda el frontend, el
    // login se salta el paso de código por email.
    public class TrustedDevice : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Token de dispositivo hasheado (nunca se guarda en texto plano).
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
