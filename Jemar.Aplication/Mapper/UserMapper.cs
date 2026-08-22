using System;
using System.Collections.Generic;
using System.Linq;
using Jemar.Domain.Entities;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class UserMapper
    {
        public static User ToUser(this CreateUserRequest request)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                RoleId = request.Role,
                IsActive = true,
                IsEmailVerified = true,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };


        }

        public static UserResponse ToUserResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString(),
                IsActive = user.IsActive
            };
        }

        public static List<UserResponse> ToUserResponseList(this List<User> users)
        {
            return users.Select(u => u.ToUserResponse()).ToList();
        }
    }
}