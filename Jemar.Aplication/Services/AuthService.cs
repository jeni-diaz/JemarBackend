using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jemar.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse?> SignInAsync(SignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Email y contraseña son requeridos.");

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Email o contraseña incorrectos.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedException("Email o contraseña incorrectos.");

            var token = _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role?.Name.ToString() ?? ((UserRole)user.RoleId).ToString(),
                UserId = user.Id
            };
        }

        public async Task<AuthResponse> SignUpAsync(SignUpRequest request)
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

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = request.ToUser();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var saved = await _userRepository.AddAsync(user);

            var token = _tokenService.GenerateToken(saved);

            return new AuthResponse
            {
                Token = token,
                Email = saved.Email,
                Role = ((UserRole)saved.RoleId).ToString(),
                UserId = saved.Id
            };
        }
    }
}
