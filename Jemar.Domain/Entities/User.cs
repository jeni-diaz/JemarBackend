using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Domain.Enums;

namespace Jemar.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }


        public Client? Client { get; set; }
        public Employee? Employee { get; set; }
        public SuperAdmin? SuperAdmin { get; set; }

    }
}
