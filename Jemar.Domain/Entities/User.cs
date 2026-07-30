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

        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }

        public bool IsEmailVerified { get; set; }

        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiresAt { get; set; }

        public ICollection<Shipment> CreatedShipments { get; set; } = new List<Shipment>();
        public ICollection<Shipment> OnBehalfShipments { get; set; } = new List<Shipment>();
        public ICollection<Inquiry> CreatedInquiries { get; set; } = new List<Inquiry>();
        public ICollection<Inquiry> RespondedInquiries { get; set; } = new List<Inquiry>();
    }
}