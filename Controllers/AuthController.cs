using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using VoiceAPI.Data;
using VoiceAPI.DTOs;
using VoiceAPI.Hubs;
using VoiceAPI.Services;
using VoiceAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AgentContext _db;               // FIX 1
        private readonly PBXManager _pbx;
        private readonly ExtensionAllocator _ext;
        private readonly IConfiguration _config;
        private readonly IHubContext<EventsHub> _hub;

        public AuthController(
            AgentContext db,                            // FIX 2
            PBXManager pbx,
            ExtensionAllocator ext,
            IConfiguration config,
            IHubContext<EventsHub> hub)
        {
            _db = db;
            _pbx = pbx;
            _ext = ext;
            _config = config;
            _hub = hub;
        }

        // ============================================================
        //  LOGIN DE AGENTE
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req == null)
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = "Body inválido"
                });
            }

            // --------------------------------------------------------
            // 1) VALIDAR EN BD (YA FUNCIONA)
            // --------------------------------------------------------
            var agente = _db.UsuariosTelefonia
                .FirstOrDefault(a =>
                    a.IdUsuario == req.idUsuario &&
                    a.IdAgente == req.idAgente);

            if (agente == null)
            {
                return Unauthorized(new
                {
                    resultado = "ERROR",
                    mensaje = "Usuario no registrado en CT"
                });
            }

            // --------------------------------------------------------
            // 2) PBX POR CLIENTe
            // --------------------------------------------------------
            var pbx = _pbx.GetPBXByCliente(req.cliente ?? "");
            if (pbx == null)
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = $"El cliente '{req.cliente}' no tiene PBX asignada"
                });
            }

            string host = pbx["Host"] ?? "";
            string wssPort = pbx["WssPort"] ?? "8089";
            string sipDomain = pbx["SipDomain"] ?? host;

            // --------------------------------------------------------
            // 3) EXTENSIÓN
            // --------------------------------------------------------
            string extension;

            if (agente.Rol.Equals("administrativo", StringComparison.OrdinalIgnoreCase))
            {
                extension = agente.Interno!;
            }
            else
            {
                extension = await _ext.GetFreeExtension(pbx);
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = "No hay internos libres para asignar"
                });
            }

            // --------------------------------------------------------
            // 4) PROVISION WEBRTC
            // --------------------------------------------------------
            var provision = new
            {
                usuario = agente.Nombre,
                idUsuario = agente.IdUsuario,
                agente = agente.IdAgente,
                servicio = req.servicio,
                rol = agente.Rol,
                extension = extension,
                SipPassword = $"{extension}PSW",
                wssServer = host,
                wssPort = wssPort,
                sipDomain = sipDomain
            };

            // --------------------------------------------------------
            // 5) EVENTO prelogin
            // --------------------------------------------------------
            await _hub.Clients.Group("prelogin")
                .SendAsync("agentLogin", provision);

            // --------------------------------------------------------
            // 6) JWT
            // --------------------------------------------------------
            var keyBytes = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"] ?? "");
            var claims = new List<Claim>
            {
                new Claim("usuario", agente.Nombre ?? ""),
                new Claim("idUsuario", agente.IdUsuario ?? ""),
                new Claim("idAgente", agente.IdAgente),
                new Claim("cliente", req.cliente ?? ""),
                new Claim("servicio", req.servicio ?? ""),
                new Claim("rol", agente.Rol ?? ""),
                new Claim("extension", extension)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // --------------------------------------------------------
            // 7) EVENTO relogin
            // --------------------------------------------------------
            await _hub.Clients.Group($"agente:{agente.IdAgente}")
                .SendAsync("agentReLogin", provision);

            // --------------------------------------------------------
            // 8) RESPUESTA FINAL
            // --------------------------------------------------------
            return Ok(new
            {
                resultado = "OK",
                mensaje = "Login exitoso",
                jwt = jwt,
                extension = extension,
                sipPassword = $"{extension}PSW",
                agente = agente.IdAgente,
                servicio = req.servicio,
                rol = agente.Rol,
                wssServer = host,
                wssPort = wssPort,
                sipDomain = sipDomain
            });
        }
    }
}

