using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VoiceAPI.Models.Auth;
using VoiceAPI.Services;
using VoiceAPI.Utils;
using VoiceAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VoiceAPI.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwt;
        private readonly IHubContext<EventsHub> _hub;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            JwtService jwt,
            IHubContext<EventsHub> hub,
            ILogger<AuthController> logger)
        {
            _jwt = jwt;
            _hub = hub;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("🔐 LOGIN RECIBIDO");
            _logger.LogInformation("Usuario     : {0}", req.Usuario);
            _logger.LogInformation("IdUsuario   : {0}", req.IdUsuario);
            _logger.LogInformation("IdAgente    : {0}", req.IdAgente);
            _logger.LogInformation("Servicio    : {0}", req.Servicio);
            _logger.LogInformation("────────────────────────────────────────────");

            // PBX por servicio (tu lógica original)
            var (pbx, clienteDetectado) = ServicioHelper.GetClusterByService(req.Servicio);

            if (pbx == null)
            {
                _logger.LogError("❌ Servicio {0} NO pertenece a ninguna PBX", req.Servicio);
                return BadRequest(new { ok = false, error = "Servicio no pertenece a ninguna PBX" });
            }

            _logger.LogInformation("✔ Servicio {0} → PBX={1} Cliente={2}",
               req.Servicio, pbx.Id, clienteDetectado);

            // JWT
            string token = _jwt.GenerateToken(
                req.Usuario,
                req.IdUsuario,
                req.IdAgente,
                req.Servicio,
                clienteDetectado,
                pbx.Id,
                req.Rol
            );

            _logger.LogInformation("✔ JWT generado correctamente.");
            _logger.LogInformation("🔁 Preparando PROVISIONING…");

            // PROVISIONING
            var payload = new
            {
                evento = "auth.provisioning",
                usuario = req.Usuario,
                idUsuario = req.IdUsuario,
                idAgente = req.IdAgente,
                servicio = req.Servicio,
                cliente = clienteDetectado,
                rol = req.Rol,
                pbx = pbx.Id,
                host = pbx.Host,
                sipDomain = pbx.SipDomain,
                wss = pbx.WssPort,
                token = token
            };

            // 1) prelogin (primera carga)
            await _hub.Clients.Group("prelogin")
                .SendAsync("provision", payload);
            _logger.LogInformation("✔ Provision enviado → prelogin");

            // 2) instancia (si existe)
            if (!string.IsNullOrWhiteSpace(req.InstanceId))
            {
                await _hub.Clients.Group($"instancia:{req.InstanceId}")
                    .SendAsync("provision", payload);

                _logger.LogInformation("✔ Provision enviado → instancia:{0}", req.InstanceId);
            }

            // 3) agente (después de BindAgent)
            await _hub.Clients.Group($"AGENTE_{req.IdAgente}")
                .SendAsync("provision", payload);

            _logger.LogInformation("✔ Provision enviado → agente:{0}", req.IdAgente);
            _logger.LogInformation("────────────────────────────────────────────");

            return Ok(new
            {
                ok = true,
                jwt = token,
                pbx = pbx.Id,
                servicio = req.Servicio
            });
        }
    }
}

