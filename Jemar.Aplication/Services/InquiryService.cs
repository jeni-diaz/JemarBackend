using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Services
{
    public class InquiryService : IInquiryService
    {
        private readonly IInquiryRepository _inquiryRepository;
        private readonly IUserRepository _userRepository;

        public InquiryService(IInquiryRepository inquiryRepository, IUserRepository userRepository)
        {
            _inquiryRepository = inquiryRepository;
            _userRepository = userRepository;
        }

        public async Task<List<InquiryResponse>> GetAllAsync(Guid currentUserId, string currentUserRole)
        {
            List<Inquiry> inquiries;
            if (currentUserRole == UserRoleEnum.Client.ToString())
            {
                inquiries = await _inquiryRepository.GetByClientIdAsync(currentUserId);
            }
            else
            {
                inquiries = await _inquiryRepository.GetAllAsync();
            }

            return inquiries.ToInquiryResponseList();
        }

        public async Task<InquiryResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(id);
            if (inquiry == null)
                return null;

            if (currentUserRole == UserRoleEnum.Client.ToString() && inquiry.CreatedByUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("No tiene autorización para ver esta consulta.");
            }

            return inquiry.ToInquiryResponse();
        }

        public async Task<InquiryResponse> CreateAsync(CreateInquiryRequest request, Guid clientId)
        {
            var user = await _userRepository.GetByIdAsync(clientId);
            if (user == null)
                throw new NotFoundException("Cliente no encontrado.");

            if (user.RoleId != (int)UserRoleEnum.Client)
                throw new UnauthorizedException("Solo los clientes pueden crear consultas.");

            var inquiry = request.ToInquiry(user);
            var saved = await _inquiryRepository.AddAsync(inquiry);
            return saved.ToInquiryResponse();
        }

        public async Task<bool> RespondAsync(Guid id, RespondInquiryRequest request, Guid currentUserId, string currentUserRole)
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(id);
            if (inquiry == null)
                return false;

            if (currentUserRole == UserRoleEnum.Employee.ToString() || currentUserRole == UserRoleEnum.SuperAdmin.ToString())
            {
                inquiry.Response = request.Response;
                inquiry.Status = InquiryStatusEnum.Answered;
                inquiry.RespondedByUserId = currentUserId;
                inquiry.UpdatedDateTime = DateTime.UtcNow;

                await _inquiryRepository.UpdateAsync(inquiry);
                return true;
            }
            else if (currentUserRole == UserRoleEnum.Client.ToString())
            {
                if (inquiry.CreatedByUserId != currentUserId)
                    throw new UnauthorizedAccessException("No está autorizado para responder a esta consulta.");

                if (inquiry.Status != InquiryStatusEnum.Answered)
                    throw new ArgumentException("Solo puede responder a las consultas que hayan sido atendidas por nuestro personal.");

                inquiry.ClientReply = request.Response;
                inquiry.Status = InquiryStatusEnum.InProgress;
                inquiry.UpdatedDateTime = DateTime.UtcNow;

                await _inquiryRepository.UpdateAsync(inquiry);
                return true;
            }

            return false;
        }

        public async Task<bool> CloseAsync(Guid id)
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(id);
            if (inquiry == null)
                return false;

            inquiry.Status = InquiryStatusEnum.Closed;
            inquiry.UpdatedDateTime = DateTime.UtcNow;

            await _inquiryRepository.UpdateAsync(inquiry);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole)
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(id);
            if (inquiry == null)
                return false;

            if (currentUserRole == UserRoleEnum.Client.ToString())
            {
                if (inquiry.CreatedByUserId != currentUserId)
                    throw new UnauthorizedAccessException("No tiene autorización para eliminar esta consulta.");

                if (inquiry.Status != InquiryStatusEnum.New)
                    throw new ArgumentException("Los clientes solo pueden eliminar consultas que aún no han sido respondidas.");
            }

            await _inquiryRepository.DeleteAsync(id);
            return true;
        }
    }
}