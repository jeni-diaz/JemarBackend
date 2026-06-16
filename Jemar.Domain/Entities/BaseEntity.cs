using System;

namespace Jemar.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }
    }
}