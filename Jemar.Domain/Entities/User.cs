namespace Jemar.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public UserRole Role { get; set; } = null!;
        public bool IsActive { get; set; }

        // Recuperación de contraseña (código de un solo uso, guardado hasheado)
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }

        // Verificación de email: el usuario confirma el código que le enviamos
        // al registrarse. Hasta que no lo haga, no puede iniciar sesión.
        public bool IsEmailVerified { get; set; }

        // Doble factor opcional por email (OTP guardado hasheado). Desactivado por
        // defecto: el código de un solo uso se usa para verificar el email al
        // registrarse, no en cada inicio de sesión.
        public bool IsTwoFactorEnabled { get; set; }
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiresAt { get; set; }

        public ICollection<Shipment> CreatedShipments { get; set; } = new List<Shipment>();
        public ICollection<Shipment> OnBehalfShipments { get; set; } = new List<Shipment>();
        public ICollection<Inquiry> CreatedInquiries { get; set; } = new List<Inquiry>();
        public ICollection<Inquiry> RespondedInquiries { get; set; } = new List<Inquiry>();
    }
}