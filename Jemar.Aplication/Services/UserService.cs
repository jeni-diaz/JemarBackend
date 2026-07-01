using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;
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
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ValidationException("El nombre es requerido.");

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ValidationException("El apellido es requerido.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ValidationException("El email es requerido.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("La contraseña es requerida.");

            if (!Enum.IsDefined(typeof(UserRoleEnum), request.Role))
                throw new ValidationException("El rol especificado no es válido.");

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = request.ToUser();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var saved = await _userRepository.AddAsync(user);
            return saved.ToUserResponse();
        }

        public async Task<bool> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request)
        {
            if (!Enum.IsDefined(typeof(UserRoleEnum), request.RoleId))
                throw new ValidationException("El rol especificado no es válido.");

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