using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class Inquiry : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Response { get; set; }
        public string? ClientReply { get; set; }
        public InquiryStatusEnum Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public Guid? RespondedByUserId { get; set; }
        public User? RespondedByUser { get; set; }
    }
}