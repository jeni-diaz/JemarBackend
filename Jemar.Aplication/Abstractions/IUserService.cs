using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAllAsync();
        Task<UserResponse?> GetByIdAsync(Guid id);
        Task<UserResponse> CreateAsync(CreateUserRequest request);
        Task<bool> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request);
    }
}
