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
            User user;
            if (request.UserRole == (int)UserRoleEnum.Client)
            {
                user = new Client { RegistrationDate = DateTime.UtcNow };
            }
            else if (request.UserRole == (int)UserRoleEnum.Employee)
            {
                user = new Employee { HireDate = DateTime.UtcNow, Position = "Staff" };
            }
            else if (request.UserRole == (int)UserRoleEnum.SuperAdmin)
            {   
                user = new SuperAdmin { CreatedAt = DateTime.UtcNow };
            }
            else
            {
                user = new User();
            }

            user.Id = Guid.NewGuid();
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Password = request.Password;
            user.RoleId = request.UserRole;
            user.IsActive = true;

            return user;
        }

        public static UserResponse ToUserResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserRole = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString()
            };
        }

        public static List<UserResponse> ToUserResponseList(this List<User> users)
        {
            return users.Select(u => u.ToUserResponse()).ToList();
        }
    }
}