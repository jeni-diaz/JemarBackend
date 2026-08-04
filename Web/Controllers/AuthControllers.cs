using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System.Threading.Tasks;

namespace Jemar.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";

        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<AuthResponse>> Login(SignInRequest request)
        {
            var response = await _authService.SignInAsync(request);
            if (response == null)
            {
                return Unauthorized("Email o contraseña incorrectos.");
            }

            SetRefreshTokenCookie(response.RefreshTokenPlaintext);
            return Ok(response);
        }

        [HttpPost("verify-2fa")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<AuthResponse>> VerifyTwoFactor(VerifyTwoFactorRequest request)
        {
            var response = await _authService.VerifyTwoFactorAsync(request);
            SetRefreshTokenCookie(response.RefreshTokenPlaintext);
            return Ok(response);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized("Sesión inválida, iniciá sesión de nuevo.");

            var response = await _authService.RefreshAsync(refreshToken);
            SetRefreshTokenCookie(response.RefreshTokenPlaintext);
            return Ok(response);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<ActionResult<MessageResponse>> Logout()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            await _authService.LogoutAsync(refreshToken);
            Response.Cookies.Delete(RefreshTokenCookieName, BuildCookieOptions(DateTimeOffset.UtcNow));
            return Ok(new MessageResponse { Message = "Sesión cerrada." });
        }

        private void SetRefreshTokenCookie(string? refreshTokenPlaintext)
        {
            if (string.IsNullOrEmpty(refreshTokenPlaintext))
                return;

            Response.Cookies.Append(
                RefreshTokenCookieName,
                refreshTokenPlaintext,
                BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(30)));
        }

        private CookieOptions BuildCookieOptions(DateTimeOffset expires) => new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/api/auth",
            Expires = expires
        };

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(SignUpRequest request)
        {
            var response = await _authService.SignUpAsync(request);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request);
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponse>> Me()
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponse>> UpdateMe(UpdateProfileRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var profile = await _authService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }

        [HttpPut("me/password")]
        [Authorize]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            await _authService.ChangePasswordAsync(userId, request);
            return NoContent();
        }
    }
}
