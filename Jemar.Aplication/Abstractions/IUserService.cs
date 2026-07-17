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
        Task<UserResponse?> GetByEmailAsync(string email);
        Task<UserResponse> CreateAsync(CreateUserRequest request);
        Task<UserResponse> CreateClientAsync(SignUpRequest request);
        Task<bool> UpdateRoleAsync(UpdateUserRoleRequest request);
        Task<bool> DeleteAsync(string email);
    }
}
