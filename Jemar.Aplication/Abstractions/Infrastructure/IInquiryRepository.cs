using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Domain.Entities;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions.Infrastructure
{
    public interface IInquiryRepository : IBaseRepository<Inquiry>
    {
        Task<Inquiry?> GetInquiryAsync()
    }
}
