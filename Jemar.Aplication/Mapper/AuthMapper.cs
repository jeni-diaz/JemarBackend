using Jemar.Aplication.Requests;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    public static class AuthMapper
    {
        public static User ToUser(this SignUpRequest request)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim().ToLower(),
                Password = request.Password,
                RoleId = (int)UserRoleEnum.Client,
                IsActive = true,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };
        }
    }
}
