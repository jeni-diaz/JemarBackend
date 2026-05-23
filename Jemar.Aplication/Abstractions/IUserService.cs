using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetAll();
        Task<UserResponse?> GetById(Guid id);
        Task<UserResponse> Create(CreateUserRequest request);
    }
}
