using Jemar.Aplication.Abstractions;
using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
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
            var secret = _config["Jwt:Secret"] ?? "a-la-grande-le-puse-cuca";

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
                new Claim(ClaimTypes.Role, user.Role?.Name.ToString() ?? ((UserRole)user.RoleId).ToString()),
                new Claim("role", user.Role?.Name.ToString() ?? ((UserRole)user.RoleId).ToString())
            };

            var expirationHoursVal = _config["Jwt:ExpirationHours"] ?? "8";
            double.TryParse(expirationHoursVal, out var expHours);
            if (expHours <= 0) expHours = 8;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "JemarApi",
                audience: _config["Jwt:Audience"] ?? "JemarClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
