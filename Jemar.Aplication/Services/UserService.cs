using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Mapper;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly FluentValidation.IValidator<CreateUserRequest> _createUserValidator;
        private readonly FluentValidation.IValidator<SignUpRequest> _signUpValidator;

        public UserService(
            IUserRepository userRepository,
            FluentValidation.IValidator<CreateUserRequest> createUserValidator,
            FluentValidation.IValidator<SignUpRequest> signUpValidator)
        {
            _userRepository = userRepository;
            _createUserValidator = createUserValidator;
            _signUpValidator = signUpValidator;
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
            var validation = await _createUserValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.First().ErrorMessage);

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = request.ToUser();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var saved = await _userRepository.AddAsync(user);
            return saved.ToUserResponse();
        }

        public async Task<UserResponse> CreateClientAsync(SignUpRequest request)
        {
            var validation = await _signUpValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.First().ErrorMessage);

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