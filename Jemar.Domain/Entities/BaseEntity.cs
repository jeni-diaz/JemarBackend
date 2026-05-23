using System;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }
}

