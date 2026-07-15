using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jemar.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private const int TwoFactorCodeMinutes = 5;
        private const int PasswordResetCodeMinutes = 15;
        private const int EmailVerificationCodeMinutes = 15;

        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<AuthResponse?> SignInAsync(SignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Email y contraseña son requeridos.");

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Email o contraseña incorrectos.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedException("Email o contraseña incorrectos.");

            // El email tiene que estar verificado para poder iniciar sesión. Si no
            // lo está (registro sin completar), reenviamos el código y le pedimos al
            // frontend que muestre el paso de verificación.
            if (!user.IsEmailVerified)
            {
                var verificationCode = GenerateNumericCode();
                user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);
                user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes);
                user.UpdatedDateTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                await _emailService.SendAsync(
                    user.Email,
                    "Verificá tu email - Jemar Envíos",
                    BuildEmailVerificationEmail(user.FirstName, verificationCode, EmailVerificationCodeMinutes));

                return new AuthResponse
                {
                    Token = string.Empty,
                    Email = user.Email,
                    Role = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString(),
                    UserId = user.Id,
                    RequiresEmailVerification = true
                };
            }

            // Si el usuario no tiene 2FA habilitado, devolvemos el token directo.
            if (!user.IsTwoFactorEnabled)
                return BuildAuthResponse(user, _tokenService.GenerateToken(user));

            // Segundo factor: generamos un código, lo guardamos hasheado y lo enviamos por email.
            var code = GenerateNumericCode();
            user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(code);
            user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(TwoFactorCodeMinutes);
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _emailService.SendAsync(
                user.Email,
                "Tu código de verificación - Jemar Envíos",
                BuildTwoFactorEmail(user.FirstName, code, TwoFactorCodeMinutes));

            return new AuthResponse
            {
                Token = string.Empty,
                Email = user.Email,
                Role = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString(),
                UserId = user.Id,
                RequiresTwoFactor = true
            };
        }

        public async Task<AuthResponse> VerifyTwoFactorAsync(VerifyTwoFactorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                throw new ValidationException("Email y código son requeridos.");

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Código inválido o expirado.");

            if (string.IsNullOrEmpty(user.TwoFactorCode) ||
                user.TwoFactorCodeExpiresAt == null ||
                user.TwoFactorCodeExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Código inválido o expirado.");

            if (!BCrypt.Net.BCrypt.Verify(request.Code.Trim(), user.TwoFactorCode))
                throw new UnauthorizedException("Código inválido o expirado.");

            // Código consumido: lo limpiamos para que no se pueda reutilizar. Al
            // verificar el código damos por confirmado el email (sirve tanto para
            // la verificación de registro como para el 2FA opcional de login).
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiresAt = null;
            user.IsEmailVerified = true;
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return BuildAuthResponse(user, _tokenService.GenerateToken(user));
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

            ValidatePasswordStrength(request.Password);

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = request.ToUser();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // El usuario queda registrado pero sin verificar. Generamos un código,
            // lo guardamos hasheado y se lo enviamos por email. Recién cuando lo
            // confirma (verify-2fa) puede iniciar sesión.
            var code = GenerateNumericCode();
            user.IsEmailVerified = false;
            user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(code);
            user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes);

            var saved = await _userRepository.AddAsync(user);

            await _emailService.SendAsync(
                saved.Email,
                "Verificá tu email - Jemar Envíos",
                BuildEmailVerificationEmail(saved.FirstName, code, EmailVerificationCodeMinutes));

            return new AuthResponse
            {
                Token = string.Empty,
                Email = saved.Email,
                Role = ((UserRoleEnum)saved.RoleId).ToString(),
                UserId = saved.Id,
                RequiresEmailVerification = true
            };
        }

        public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            // Respuesta genérica siempre, exista o no el email, para no filtrar
            // qué correos están registrados (enumeración de usuarios).
            var genericResponse = new MessageResponse
            {
                Message = "Si el email está registrado, te enviamos un código para restablecer tu contraseña."
            };

            if (string.IsNullOrWhiteSpace(request.Email) ||
                !Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return genericResponse;

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null || !user.IsActive)
                return genericResponse;

            var code = GenerateNumericCode();
            user.PasswordResetCode = BCrypt.Net.BCrypt.HashPassword(code);
            user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetCodeMinutes);
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _emailService.SendAsync(
                user.Email,
                "Restablecé tu contraseña - Jemar Envíos",
                BuildPasswordResetEmail(user.FirstName, code, PasswordResetCodeMinutes));

            return genericResponse;
        }

        public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                !Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ValidationException("El email no tiene un formato válido.");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ValidationException("El código es requerido.");

            ValidatePasswordStrength(request.NewPassword);

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Código inválido o expirado.");

            if (string.IsNullOrEmpty(user.PasswordResetCode) ||
                user.PasswordResetCodeExpiresAt == null ||
                user.PasswordResetCodeExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Código inválido o expirado.");

            if (!BCrypt.Net.BCrypt.Verify(request.Code.Trim(), user.PasswordResetCode))
                throw new UnauthorizedException("Código inválido o expirado.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiresAt = null;
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new MessageResponse
            {
                Message = "Tu contraseña fue actualizada correctamente."
            };
        }

        private AuthResponse BuildAuthResponse(User user, string token) => new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Role = user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString(),
            UserId = user.Id,
            RequiresTwoFactor = false
        };

        private static void ValidatePasswordStrength(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("La contraseña es requerida.");

            var letters = Regex.Matches(password, @"[a-zA-Z]").Count;
            var digits = Regex.Matches(password, @"[0-9]").Count;
            if (letters < 3 || digits < 1)
                throw new ValidationException("La contraseña debe tener al menos 3 letras y 1 número.");
        }

        // Código numérico de 6 dígitos generado con un RNG criptográficamente seguro.
        private static string GenerateNumericCode()
        {
            var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return number.ToString("D6");
        }

        private static string BuildEmailVerificationEmail(string firstName, string code, int minutes) =>
            $@"<div style=""font-family:Arial,sans-serif;color:#222"">
                <h2>Verificá tu email</h2>
                <p>Hola {firstName},</p>
                <p>¡Gracias por registrarte en Jemar Envíos! Para activar tu cuenta, ingresá este código:</p>
                <p style=""font-size:28px;font-weight:bold;letter-spacing:4px"">{code}</p>
                <p>El código vence en {minutes} minutos. Si no creaste esta cuenta, ignorá este correo.</p>
                <p>— Jemar Envíos</p>
            </div>";

        private static string BuildTwoFactorEmail(string firstName, string code, int minutes) =>
            $@"<div style=""font-family:Arial,sans-serif;color:#222"">
                <h2>Verificación de inicio de sesión</h2>
                <p>Hola {firstName},</p>
                <p>Tu código de verificación es:</p>
                <p style=""font-size:28px;font-weight:bold;letter-spacing:4px"">{code}</p>
                <p>El código vence en {minutes} minutos. Si no intentaste iniciar sesión, ignorá este correo.</p>
                <p>— Jemar Envíos</p>
            </div>";

        private static string BuildPasswordResetEmail(string firstName, string code, int minutes) =>
            $@"<div style=""font-family:Arial,sans-serif;color:#222"">
                <h2>Restablecer contraseña</h2>
                <p>Hola {firstName},</p>
                <p>Recibimos una solicitud para restablecer tu contraseña. Usá este código:</p>
                <p style=""font-size:28px;font-weight:bold;letter-spacing:4px"">{code}</p>
                <p>El código vence en {minutes} minutos. Si no solicitaste el cambio, ignorá este correo.</p>
                <p>— Jemar Envíos</p>
            </div>";
    }
}
