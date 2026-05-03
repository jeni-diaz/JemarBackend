using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Aplication.Abstractions
{
    public interface IUserService
    {
        List<UserResponse> GetAll();
        UserResponse? GetById(Guid id);
        UserResponse Create(CreateUserRequest reequest);
    }
}
