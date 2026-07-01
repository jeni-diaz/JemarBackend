using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;
using System.Text.RegularExpressions;

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

        public async Task<UserResponse?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("El email es requerido.");

            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            var user = await _userRepository.GetByEmailAsync(email.Trim());

            if (user == null)
                throw new NotFoundException("No existe un usuario con ese email.");

            return user.ToUserResponse();
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length <= 3)
                throw new ValidationException("El nombre debe tener más de 3 letras.");

            if (!Regex.IsMatch(request.FirstName.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
                throw new ValidationException("El nombre solo puede contener letras.");

            if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length <= 3)
                throw new ValidationException("El apellido debe tener más de 3 letras.");

            if (!Regex.IsMatch(request.LastName.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
                throw new ValidationException("El apellido solo puede contener letras.");

            if (string.IsNullOrWhiteSpace(request.Email) ||
                !Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("La contraseña es requerida.");

            var letters = Regex.Matches(request.Password, @"[a-zA-Z]").Count;
            var digits = Regex.Matches(request.Password, @"[0-9]").Count;

            if (letters < 3 || digits < 1)
                throw new ValidationException("La contraseña debe tener al menos 3 letras y 1 número.");

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

        public async Task<bool> UpdateRoleAsync(UpdateUserRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ValidationException("El email es requerido.");

            if (!Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            if (!Enum.IsDefined(typeof(UserRoleEnum), request.RoleId))
                throw new ValidationException("El rol especificado no es válido.");

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());

            if (user == null)
                throw new NotFoundException("No existe un usuario con ese email.");

            user.RoleId = request.RoleId;
            user.UpdatedDateTime = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}