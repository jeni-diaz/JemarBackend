using System;
using System.Collections.Generic;
using System.Text;
using Jemar.Domain.Entities;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class UserMapper
    {
        public static User ToEntity(CreateUserRequest request)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                Role = (UserRole)request.Role,
                IsActive = true
            };
        }

        public static UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public static List<UserResponse> ToListResponse(List<User> users)
        {
            return users.Select(ToResponse).ToList();
        }
    }
}