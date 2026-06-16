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
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public bool IsActive { get; set; }

        // Navegación: envíos como cliente
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        // Navegación: envíos asignados como empleado
        public ICollection<Shipment> AssignedShipments { get; set; } = new List<Shipment>();

        // Navegación: consultas como cliente
        public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

        // Navegación: consultas asignadas como empleado
        public ICollection<Inquiry> AssignedInquiries { get; set; } = new List<Inquiry>();
    }
}
