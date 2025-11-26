using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VoiceAPI.Hubs;
using VoiceAPI.Models.Provisioning;
using VoiceAPI.Utils;

namespace VoiceAPI.Controllers.Provisioning
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvisioningController : ControllerBase
    {
        private readonly ILogger<ProvisioningController> _logger;
        private readonly IHubContext<EventsHub> _hub;
        private readonly ServicioHelper _servicioHelper;

        public ProvisioningController(
            ILogger<ProvisioningController> logger,
            IHubContext<EventsHub> hub,
            ServicioHelper servicioHelper)
        {
            _logger = logger;
            _hub = hub;
            _servicioHelper = servicioHelper;
        }

        // ===============================================
        // POST /api/provisioning/send
        // Enviar provisioning manual / pruebas
        // ===============================================
        [HttpPost("send")]
        public async Task<IActionResult> SendProvision([FromBody] ProvisionRequest req)
        {
            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("📦 PROVISIONING RECIBIDO");
            _logger.LogInformation("→ idAgente     : {agente}", req.IdAgente);
            _logger.LogInformation("→ Cliente      : {cliente}", req.Cliente);
            _logger.LogInformation("→ Servicio     : {servicio}", req.Servicio);
            _logger.LogInformation("→ InstanceId   : {instance}", req.InstanceId);
            _logger.LogInformation("────────────────────────────────────────────");

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(req.InstanceId))
            {
                _logger.LogError("❌ ERROR: InstanceId no proporcionado en el provisioning.");
                return BadRequest("InstanceId obligatorio");
            }

            // Grupo de destino en SignalR
            string instanceGroup = $"instancia:{req.InstanceId}";

            var payload = new
            {
                evento = "provisioning.manual",
                agente = req.IdAgente,
                cliente = req.Cliente,
                servicio = req.Servicio
            };

            _logger.LogInformation("📤 Enviando provisioning manual → {grp}", instanceGroup);
            _logger.LogInformation("Payload → {@payload}", payload);

            await _hub.Clients.Group(instanceGroup)
                .SendAsync("provisioningLogin", payload);

            _logger.LogInformation("✔ PROVISIONING enviado correctamente.");
            _logger.LogInformation("────────────────────────────────────────────");

            return Ok(new { status = "ok", enviadoA = instanceGroup });
        }


        // ===============================================
        // POST /api/provisioning/byAgent
        // Enviar provisioning dirigido a un agente (grupo agente)
        // ===============================================
        [HttpPost("byAgent")]
        public async Task<IActionResult> SendToAgent([FromBody] ProvisionRequest req)
        {
            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("📦 PROVISIONING POR AGENTE RECIBIDO");
            _logger.LogInformation("→ idAgente     : {agente}", req.IdAgente);
            _logger.LogInformation("→ Cliente      : {cliente}", req.Cliente);
            _logger.LogInformation("→ Servicio     : {servicio}", req.Servicio);
            _logger.LogInformation("────────────────────────────────────────────");

            if (string.IsNullOrWhiteSpace(req.IdAgente))
            {
                _logger.LogError("❌ ERROR: IdAgente no proporcionado.");
                return BadRequest("IdAgente obligatorio");
            }

            string agentGroup = $"agente:{req.IdAgente}";

            var payload = new
            {
                evento = "provisioning.agente",
                agente = req.IdAgente,
                cliente = req.Cliente,
                servicio = req.Servicio
            };

            _logger.LogInformation("📤 Enviando provisioning por agente → {grp}", agentGroup);
            _logger.LogInformation("Payload → {@payload}", payload);

            await _hub.Clients.Group(agentGroup)
                .SendAsync("provisioningLogin", payload);

            _logger.LogInformation("✔ PROVISIONING enviado correctamente.");
            _logger.LogInformation("────────────────────────────────────────────");

            return Ok(new { status = "ok", enviadoA = agentGroup });
        }



        // ===============================================
        // POST /api/provisioning/instance
        // Enviar provisioning directamente a una instancia
        // ===============================================
        [HttpPost("instance")]
        public async Task<IActionResult> SendToInstance([FromBody] ProvisionRequest req)
        {
            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("📦 PROVISIONING DIRECTO A INSTANCIA RECIBIDO");
            _logger.LogInformation("→ InstanceId : {instance}", req.InstanceId);
            _logger.LogInformation("→ idAgente   : {agente}", req.IdAgente);
            _logger.LogInformation("────────────────────────────────────────────");

            if (string.IsNullOrWhiteSpace(req.InstanceId))
            {
                _logger.LogError("❌ ERROR: InstanceId no proporcionado.");
                return BadRequest("InstanceId obligatorio");
            }

            string instanceGroup = $"instancia:{req.InstanceId}";

            var payload = new
            {
                evento = "provisioning.instance",
                agente = req.IdAgente,
                cliente = req.Cliente,
                servicio = req.Servicio
            };

            _logger.LogInformation("📤 Enviando provisioning instancia → {grp}", instanceGroup);
            _logger.LogInformation("Payload → {@payload}", payload);

            await _hub.Clients.Group(instanceGroup)
                .SendAsync("provisioningLogin", payload);

            _logger.LogInformation("✔ PROVISIONING enviado correctamente.");
            _logger.LogInformation("────────────────────────────────────────────");

            return Ok(new { status = "ok", enviadoA = instanceGroup });
        }

    }
}

