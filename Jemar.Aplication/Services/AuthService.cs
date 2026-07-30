using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Exceptions;
using Jemar.Aplication.Mapper;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Aplication.Validation;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jemar.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private const int TwoFactorCodeMinutes = 5;
        private const int PasswordResetCodeMinutes = 15;
        private const int EmailVerificationCodeMinutes = 15;
        private const int TrustedDeviceDays = 30;
        private const int RefreshTokenDays = 30;

        private readonly IUserRepository _userRepository;
        private readonly ITrustedDeviceRepository _trustedDeviceRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly FluentValidation.IValidator<SignUpRequest> _signUpValidator;
        private readonly FluentValidation.IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ITrustedDeviceRepository trustedDeviceRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IEmailService emailService,
            FluentValidation.IValidator<SignUpRequest> signUpValidator,
            FluentValidation.IValidator<ResetPasswordRequest> resetPasswordValidator,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _trustedDeviceRepository = trustedDeviceRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _signUpValidator = signUpValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _logger = logger;
        }

        private void SendEmailInBackground(string toEmail, string subject, string htmlBody)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendAsync(toEmail, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo enviar el email a {Email}.", toEmail);
                }
            });
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

            if (!user.IsEmailVerified)
            {
                var verificationCode = GenerateNumericCode();
                user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);
                user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes);
                user.UpdatedDateTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                SendEmailInBackground(
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

            if (await IsDeviceTrustedAsync(user.Id, request.DeviceToken))
            {
                var trustedDeviceResponse = BuildAuthResponse(user, _tokenService.GenerateToken(user));
                trustedDeviceResponse.RefreshTokenPlaintext = await IssueRefreshTokenAsync(user.Id);
                return trustedDeviceResponse;
            }

            var code = GenerateNumericCode();
            user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(code);
            user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(TwoFactorCodeMinutes);
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            SendEmailInBackground(
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

            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiresAt = null;
            user.IsEmailVerified = true;
            user.UpdatedDateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var deviceToken = GenerateDeviceToken();
            await _trustedDeviceRepository.AddAsync(new TrustedDevice
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(deviceToken),
                ExpiresAt = DateTime.UtcNow.AddDays(TrustedDeviceDays)
            });

            var response = BuildAuthResponse(user, _tokenService.GenerateToken(user));
            response.DeviceToken = deviceToken;
            response.RefreshTokenPlaintext = await IssueRefreshTokenAsync(user.Id);
            return response;
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedException("Sesión inválida, iniciá sesión de nuevo.");

            var hash = HashToken(refreshToken);
            var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash);

            if (stored == null)
                throw new UnauthorizedException("Sesión inválida, iniciá sesión de nuevo.");

            if (stored.RevokedAt != null)
            {
                var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(stored.UserId);
                foreach (var active in activeTokens)
                {
                    active.RevokedAt = DateTime.UtcNow;
                    await _refreshTokenRepository.UpdateAsync(active);
                }

                throw new UnauthorizedException("Sesión inválida, iniciá sesión de nuevo.");
            }

            if (stored.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Sesión inválida, iniciá sesión de nuevo.");

            var user = await _userRepository.GetByIdAsync(stored.UserId);
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("Sesión inválida, iniciá sesión de nuevo.");

            var newRefreshToken = GenerateRefreshToken();
            var newHash = HashToken(newRefreshToken);

            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = newHash;
            await _refreshTokenRepository.UpdateAsync(stored);

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays)
            });

            var response = BuildAuthResponse(user, _tokenService.GenerateToken(user));
            response.RefreshTokenPlaintext = newRefreshToken;
            return response;
        }

        public async Task LogoutAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var stored = await _refreshTokenRepository.GetByTokenHashAsync(HashToken(refreshToken));
            if (stored == null || stored.RevokedAt != null)
                return;

            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);
        }

        public async Task<AuthResponse> SignUpAsync(SignUpRequest request)
        {
            var validation = await _signUpValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.First().ErrorMessage);

            var existing = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (existing != null)
                throw new ConflictException("Ya existe un usuario registrado con ese email.");

            var user = request.ToUser();
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var code = GenerateNumericCode();
            user.IsEmailVerified = false;
            user.TwoFactorCode = BCrypt.Net.BCrypt.HashPassword(code);
            user.TwoFactorCodeExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationCodeMinutes);

            var saved = await _userRepository.AddAsync(user);

            SendEmailInBackground(
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

            SendEmailInBackground(
                user.Email,
                "Restablecé tu contraseña - Jemar Envíos",
                BuildPasswordResetEmail(user.FirstName, code, PasswordResetCodeMinutes));

            return genericResponse;
        }

        public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var validation = await _resetPasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors.First().ErrorMessage);

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

        private static string GenerateNumericCode()
        {
            var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return number.ToString("D6");
        }

        private static string GenerateDeviceToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        private static string GenerateRefreshToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private async Task<string> IssueRefreshTokenAsync(Guid userId)
        {
            var plaintext = GenerateRefreshToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = HashToken(plaintext),
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays)
            });
            return plaintext;
        }

        private async Task<bool> IsDeviceTrustedAsync(Guid userId, string? deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
                return false;

            var devices = await _trustedDeviceRepository.GetActiveByUserIdAsync(userId);
            return devices.Any(d => BCrypt.Net.BCrypt.Verify(deviceToken, d.TokenHash));
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
