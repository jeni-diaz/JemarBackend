using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;

namespace Jemar.Aplication.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToUserResponseList();
        }

        public async Task<UserResponse?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            return user.ToUserResponse();
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var user = request.ToUser();
            var saved = await _userRepository.AddAsync(user);
            return saved.ToUserResponse();
        }

        public async Task<bool> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.RoleId = request.RoleId;
            user.UpdatedDateTime = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
