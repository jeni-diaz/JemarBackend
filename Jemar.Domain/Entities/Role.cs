using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public UserRole Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}