using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<List<Payment>> GetByShipmentIdAsync(Guid shipmentId);
        Task<Payment?> GetLatestByShipmentIdAsync(Guid shipmentId);
        Task<Payment?> GetPendingByShipmentIdAsync(Guid shipmentId);
    }
}
