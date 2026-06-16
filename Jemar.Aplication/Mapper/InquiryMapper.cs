using System;
using System.Collections.Generic;
using System.Linq;
using Jemar.Domain.Entities;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class InquiryMapper
    {
        public static Inquiry ToInquiry(this CreateInquiryRequest request, User client)
        {
            return new Inquiry
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Message = request.Message,
                Status = InquiryStatusEnum.New,
                ClientId = client.Id,
                IsDeleted = false
            };
        }

        public static InquiryResponse ToInquiryResponse(this Inquiry inquiry)
        {
            return new InquiryResponse
            {
                Id = inquiry.Id,
                FirstName = inquiry.FirstName,
                LastName = inquiry.LastName,
                Email = inquiry.Email,
                Message = inquiry.Message,
                Response = inquiry.Response,
                ClientReply = inquiry.ClientReply,
                Status = inquiry.Status.ToString(),
                CreatedAt = inquiry.CreatedAt
            };
        }

        public static List<InquiryResponse> ToInquiryResponseList(this List<Inquiry> inquiries)
        {
            return inquiries.Select(i => i.ToInquiryResponse()).ToList();
        }
    }
}
