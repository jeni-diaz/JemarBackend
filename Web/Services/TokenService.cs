using Jemar.Aplication.Abstractions;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jemar.Presentation.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var secret = _config["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret no configurado.");

            var issuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");

            var audience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience no configurado.");

            int expirationHours = 8;

            if (int.TryParse(_config["Jwt:ExpirationHours"], out var hours))
            {
                expirationHours = hours;
            }

            var role = user.Role?.Name.ToString()
                       ?? ((UserRoleEnum)user.RoleId).ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Role, role),
                new Claim("role", role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}