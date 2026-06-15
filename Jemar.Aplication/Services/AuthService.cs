using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Abstractions.Infrastructure;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Enums;
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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return null;


            if (user.Password != request.Password)
                return null;

            var token = _tokenService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role?.Name.ToString() ?? ((UserRole)user.RoleId).ToString()
            };
        }
    }
}
