using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    public interface IInquiryService
    {
        Task<List<InquiryResponse>> GetAllAsync(Guid currentUserId, string currentUserRole);
        Task<InquiryResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole);
        Task<InquiryResponse> CreateAsync(CreateInquiryRequest request, Guid clientId);
        Task<bool> RespondAsync(Guid id, RespondInquiryRequest request, Guid currentUserId, string currentUserRole);
        Task<bool> CloseAsync(Guid id);
        Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole);
    }
}
