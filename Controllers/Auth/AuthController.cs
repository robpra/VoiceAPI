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
        private readonly ServicioHelper _servicioHelper;
        private readonly IHubContext<EventsHub> _hub;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            JwtService jwt,
            ServicioHelper servicioHelper,
            IHubContext<EventsHub> hub,
            ILogger<AuthController> logger)
        {
            _jwt = jwt;
            _servicioHelper = servicioHelper;
            _hub = hub;
            _logger = logger;
        }

        // ===========================================================
        // POST /api/auth/login
        // ===========================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("🔐 LOGIN RECIBIDO");
            _logger.LogInformation("Usuario     : {0}", req.Usuario);
            _logger.LogInformation("IdUsuario   : {0}", req.IdUsuario);
            _logger.LogInformation("IdAgente    : {0}", req.IdAgente);
            _logger.LogInformation("Servicio    : {0}", req.Servicio);
            _logger.LogInformation("Cliente     : {0}", req.Cliente);
            _logger.LogInformation("Rol         : {0}", req.Rol);
            _logger.LogInformation("────────────────────────────────────────────");

            // Buscar PBX + Cliente
            var (pbx, cliente) = _servicioHelper.GetClusterAndCliente(req.Servicio);

            if (pbx == null)
            {
                _logger.LogError("❌ Servicio {0} NO pertenece a ninguna PBX", req.Servicio);
                return BadRequest(new { error = "Servicio no pertenece a ninguna PBX" });
            }

            _logger.LogInformation("✔ Servicio {0} → PBX={1} Cliente={2}", req.Servicio, pbx.Id, cliente);

            // Generar JWT
            string token = _jwt.GenerateToken(
                req.Usuario,
                req.IdUsuario,
                req.IdAgente,
                req.Servicio,
                req.Cliente,
                pbx.Id,
                req.Rol
            );

            _logger.LogInformation("✔ JWT generado correctamente.");
            _logger.LogInformation("🔁 Preparando PROVISIONING…");

            // Payload de provisioning
            var provisioningPayload = new
            {
                evento = "auth.provisioning",
                usuario = req.Usuario,
                idUsuario = req.IdUsuario,
                idAgente = req.IdAgente,
                servicio = req.Servicio,
                cliente = cliente,
                rol = req.Rol,
                pbx = pbx.Id,
                host = pbx.Host,
                sipDomain = pbx.SipDomain,
                wss = pbx.WssPort,
                token = token
            };

            // ENVÍO A PRELOGIN (si el navegador aún no conoce idAgente)
            await _hub.Clients.Group("prelogin")
                .SendAsync("provision", provisioningPayload);

            _logger.LogInformation("✔ Provision enviado al grupo prelogin");

            // ENVÍO DIRECTO AL AGENTE (una vez que se una desde el navegador)
            await _hub.Clients.Group($"AGENTE_{req.IdAgente}")
                .SendAsync("provision", provisioningPayload);

            _logger.LogInformation("✔ Provision enviado al grupo AGENTE_{0}", req.IdAgente);
            _logger.LogInformation("────────────────────────────────────────────");

            return Ok(new
            {
                ok = true,
                token = token,
                agente = req.IdAgente,
                servicio = req.Servicio,
                pbx = pbx.Id
            });
        }
    }
}

