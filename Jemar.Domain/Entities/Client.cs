using System;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Domain.Entities
{
    public class Client : User
    {
        public DateTime RegistrationDate { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        //public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    }
}
