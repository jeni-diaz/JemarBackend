using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IShipmentRepository : IBaseRepository<Shipment>
    {
        Task<List<Shipment>> GetByClientIdAsync(Guid clientId);
    }
}
