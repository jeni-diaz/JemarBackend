using System;
using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class Inquiry : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Response { get; set; }
        public string? ClientReply { get; set; }
        public InquiryStatusEnum Status { get; set; }

        public Guid ClientId { get; set; }
        public User Client { get; set; } = null!;

        public Guid? EmployeeId { get; set; }
        public User? Employee { get; set; }
    }
}
