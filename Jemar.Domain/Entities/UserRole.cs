using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class UserRole
    {
        public int Id { get; set; }
        public Enums.UserRoleEnum Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}