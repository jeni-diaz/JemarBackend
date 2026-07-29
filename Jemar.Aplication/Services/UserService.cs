using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
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
            ValidatePersonalData(request.FirstName, request.LastName, request.Email, request.Password);

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

        // Registro de un cliente hecho por un empleado desde el alta de un envío.
        // A diferencia del alta pública, no hay verificación por email: el empleado
        // lo crea y queda activo y verificado para poder operar de inmediato.
        public async Task<UserResponse> CreateClientAsync(SignUpRequest request)
        {
            ValidatePersonalData(request.FirstName, request.LastName, request.Email, request.Password);

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = (int)UserRoleEnum.Client,
                IsActive = true,
                IsEmailVerified = true,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };

            var saved = await _userRepository.AddAsync(user);
            return saved.ToUserResponse();
        }

        private static void ValidatePersonalData(string firstName, string lastName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || firstName.Trim().Length <= 3)
                throw new ValidationException("El nombre debe tener más de 3 letras.");

            if (!Regex.IsMatch(firstName.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
                throw new ValidationException("El nombre solo puede contener letras.");

            if (string.IsNullOrWhiteSpace(lastName) || lastName.Trim().Length <= 3)
                throw new ValidationException("El apellido debe tener más de 3 letras.");

            if (!Regex.IsMatch(lastName.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
                throw new ValidationException("El apellido solo puede contener letras.");

            if (string.IsNullOrWhiteSpace(email) ||
                !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("La contraseña es requerida.");

            var letters = Regex.Matches(password, @"[a-zA-Z]").Count;
            var digits = Regex.Matches(password, @"[0-9]").Count;

            if (letters < 3 || digits < 1)
                throw new ValidationException("La contraseña debe tener al menos 3 letras y 1 número.");
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

        public async Task<bool> DeleteAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("El email es requerido.");

            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            var user = await _userRepository.GetByEmailAsync(email.Trim());

            if (user == null)
                throw new NotFoundException("No existe un usuario con ese email.");

            if (user.IsDeleted)
                throw new ConflictException("El usuario ya fue eliminado.");

            if (user.RoleId == (int)UserRoleEnum.SuperAdmin)
                throw new ValidationException("No se puede eliminar el Super Admin del sistema.");

            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedDateTime = DateTime.UtcNow;
            user.UpdatedDateTime = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }


    }
}