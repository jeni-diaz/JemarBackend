using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IInquiryRepository : IBaseRepository<Inquiry>
    {
        Task<List<Inquiry>> GetByClientIdAsync(Guid clientId);
    }
}
