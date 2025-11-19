using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace VoiceAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(Dictionary<string, string> payload)
        {
            var key = _config["JwtSettings:Key"]
                ?? throw new Exception("JwtSettings:Key no está configurado.");

            var issuer = _config["JwtSettings:Issuer"] ?? "VoiceAPI";
            var audience = _config["JwtSettings:Audience"] ?? "SGC";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = payload.Select(p => new Claim(p.Key, p.Value)).ToList();

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

