using Jemar.Aplication.Abstractions;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jemar.Infrastructure.Services
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
                ?? throw new InvalidOperationException("La configuración 'Jwt:Secret' no existe.");

            var issuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("La configuración 'Jwt:Issuer' no existe.");

            var audience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("La configuración 'Jwt:Audience' no existe.");

            var expirationHoursValue = _config["Jwt:ExpirationHours"] ?? "8";

            if (!double.TryParse(expirationHoursValue, out var expirationHours))
            {
                expirationHours = 8;
            }

            if (secret.Length < 32)
                secret = secret.PadRight(32, '!');

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString()),
                new Claim("role", user.Role?.Name.ToString() ?? ((UserRoleEnum)user.RoleId).ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer, 
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}