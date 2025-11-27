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

        public string GenerateToken(
            string usuario,
            string idUsuario,
            string idAgente,
            string servicio,
            string rol,
            string pbx,
            string cliente)
        {
            var keyString = _config["JwtSettings:Key"] ?? "";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("usuario", usuario),
                new Claim("idUsuario", idUsuario),
                new Claim("idAgente", idAgente),
                new Claim("servicio", servicio),
                new Claim("rol", rol),
                new Claim("pbx", pbx),
                new Claim("cliente", cliente)
            };

            var expiry = int.Parse(_config["JwtSettings:ExpiryHours"] ?? "8");

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

