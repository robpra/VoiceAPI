using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoiceAPI.Data;
using VoiceAPI.Models.Agent;
using VoiceAPI.Models.Auth;
using VoiceAPI.Services;
using VoiceAPI.Utils;
using VoiceAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace VoiceAPI.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AgentContext _db;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;
        private readonly IHubContext<EventsHub> _hub;

        public AuthController(AgentContext db, JwtService jwt, IConfiguration config, IHubContext<EventsHub> hub)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
            _hub = hub;
        }

        // ========================================================
        //  LOGIN
        // ========================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // ---------------------------------------
            // 1) Validar campos mínimos
            // ---------------------------------------
            if (string.IsNullOrWhiteSpace(req.IdUsuario) ||
                string.IsNullOrWhiteSpace(req.Servicio) ||
                string.IsNullOrWhiteSpace(req.Cliente) ||
                string.IsNullOrWhiteSpace(req.Rol))
            {
                return BadRequest(new { status = "error", message = "Faltan datos requeridos." });
            }

            var rol = req.Rol.ToLower().Trim();

            // ---------------------------------------
            // 2) Buscar agente
            // ---------------------------------------
            var agente = await _db.UsuariosTelefonia
                .FirstOrDefaultAsync(a => a.IdUsuario == req.IdUsuario);

            if (agente == null)
            {
                return NotFound(new { status = "error", message = "Usuario no encontrado." });
            }

            // ---------------------------------------
            // 3) Validación por rol
            // ---------------------------------------
            if (rol == "administrativo")
            {
                if (string.IsNullOrWhiteSpace(agente.Interno))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "El rol administrativo requiere tener interno asignado."
                    });
                }
            }

            if (rol == "agente")
            {
                // validar servicios
                var servicios = ServicioHelper.FromJson(agente.Servicios);

                bool permitido = servicios.Any(s => s.idServicio == req.Servicio);

                if (!permitido)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "El agente no está autorizado para el servicio solicitado."
                    });
                }
            }

            // ---------------------------------------
            // 4) Buscar PBX por cliente
            // ---------------------------------------
            var pbxClusters = _config.GetSection("PBXClusters").Get<List<PBXClusterConfig>>();
            var pbx = pbxClusters.FirstOrDefault(x => x.Clientes.Contains(req.Cliente));

            if (pbx == null)
            {
                return BadRequest(new { status = "error", message = "No se encontró PBX para el cliente." });
            }

            // ---------------------------------------
            // 5) Asignar interno (agente puede estar vacío)
            // ---------------------------------------
            string internoAsignado = agente.Interno ?? "";

            // ---------------------------------------
            // 6) Armar provisioning object
            // ---------------------------------------
            var provisioning = new
            {
                usuario = agente.Nombre + " " + agente.Apellido,
                idUsuario = agente.IdUsuario,
                agente = agente.IdAgente,
                rol = agente.Rol,
                servicio = req.Servicio,
                extension = internoAsignado,
                sipPassword = $"{internoAsignado}PSW",
                wssServer = pbx.Host,
                wssPort = pbx.WssPort,
                sipDomain = pbx.SipDomain
            };

            // ---------------------------------------
            // 7) SignalR → prelogin
            // ---------------------------------------
            await _hub.Clients.Group("prelogin")
                .SendAsync("agentLogin", provisioning);

            // ---------------------------------------
            // 8) Generar JWT
            // ---------------------------------------
            string token = _jwt.GenerateToken(
                agente.Nombre + " " + agente.Apellido,
                agente.IdUsuario,
                agente.IdAgente,
                req.Servicio,
                agente.Rol,
                req.Cliente,
                agente.PbxId ?? ""
            );

            // ---------------------------------------
            // 9) Respuesta final completa
            // ---------------------------------------
            return Ok(new
            {
                status = "ok",
                jwt = token,
                provisioning = provisioning,
                pbx = new
                {
                    id = pbx.Id,
                    host = pbx.Host,
                    ariPort = pbx.AriPort,
                    wssPort = pbx.WssPort,
                    sipDomain = pbx.SipDomain
                }
            });
        }
    }
}

