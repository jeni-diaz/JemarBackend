using Jemar.Domain.Entities;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class InquiryMapper
    {
        public static Inquiry ToInquiry(this CreateInquiryRequest request, Guid? createdByUserId)
        {
            return new Inquiry
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Message = request.Message,
                Status = InquiryStatusEnum.New,
                CreatedByUserId = createdByUserId,
                IsDeleted = false,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
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
                CreatedAt = inquiry.CreatedDateTime
            };
        }

        public static List<InquiryResponse> ToInquiryResponseList(this List<Inquiry> inquiries)
        {
            return inquiries.Select(i => i.ToInquiryResponse()).ToList();
        }
    }
}