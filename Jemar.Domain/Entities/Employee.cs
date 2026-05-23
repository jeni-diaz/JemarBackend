using System;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Domain.Entities
{
    public class Employee : User
    {
        public DateTime HireDate { get; set; }
        public string Position { get; set; } = string.Empty;

        //public ICollection<Shipment>? Shipments { get; set; }
        //public ICollection<Inquiry>? Inquiries { get; set; }

    }
}
