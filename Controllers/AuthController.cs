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
        private readonly AgentDbContext _db;
        private readonly PBXManager _pbx;
        private readonly ExtensionAllocator _ext;
        private readonly IConfiguration _config;
        private readonly IHubContext<EventsHub> _hub;

        public AuthController(
            AgentDbContext db,
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
            // 1) VALIDAR EN BD
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
            // 2) PBX POR CLIENTE
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


// AGENTE → asignación dinámica
// ADMIN → usa su interno fijo
string extension;

if (agente.Rol.Equals("administrativo", StringComparison.OrdinalIgnoreCase))
{
    extension = agente.Interno!;
}
else
{
    var pbxSection = _pbx.GetPBXByCliente(req.cliente ?? "");

    if (pbxSection == null)
    {
        return BadRequest(new
        {
            resultado = "ERROR",
            mensaje = $"El cliente '{req.cliente}' no tiene PBX asignada"
        });
    }

    extension = await _ext.GetFreeExtension(pbxSection);
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
            // 4) PROVISION PARA SIGNALR
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
            // 5) EVENTO INICIAL → prelogin
            // --------------------------------------------------------
            await _hub.Clients.Group("prelogin")
                .SendAsync("agentLogin", provision);

            Console.WriteLine($"[LOGIN] Enviado agentLogin → prelogin → agente:{agente.IdAgente}");

            // --------------------------------------------------------
            // 6) GENERAR JWT
            // --------------------------------------------------------
            var tokenHandler = new JwtSecurityTokenHandler();
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

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);

            // --------------------------------------------------------
            // 7) RE-LOGIN → grupo real del agente
            // --------------------------------------------------------
            string grupoAgente = $"agente:{agente.IdAgente}";

            await _hub.Clients.Group(grupoAgente)
                .SendAsync("agentReLogin", provision);

            Console.WriteLine($"[RELOGIN] Enviado agentReLogin → {grupoAgente}");

            // --------------------------------------------------------
            // 8) RESPUESTA FINAL AL CRM
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

