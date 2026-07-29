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
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<AuthResponse>> Login(SignInRequest request)
        {
            var response = await _authService.SignInAsync(request);
            if (response == null)
            {
                return Unauthorized("Email o contraseña incorrectos.");
            }

            return Ok(response);
        }

        [HttpPost("verify-2fa")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<AuthResponse>> VerifyTwoFactor(VerifyTwoFactorRequest request)
        {
            var response = await _authService.VerifyTwoFactorAsync(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(SignUpRequest request)
        {
            var response = await _authService.SignUpAsync(request);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request);
            return Ok(response);
        }
    }
}
