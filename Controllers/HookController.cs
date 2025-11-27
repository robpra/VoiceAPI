using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using VoiceAPI.Hubs;
using VoiceAPI.Models.Hooks;
using VoiceAPI.Services;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HookController : ControllerBase
    {
        private readonly ILogger<HookController> _logger;
        private readonly IHubContext<EventsHub> _hub;
        private readonly IHttpClientFactory _http;
        private readonly VoiceLogger _vlog;

        public HookController(
            ILogger<HookController> logger,
            IHubContext<EventsHub> hub,
            IHttpClientFactory http,
            VoiceLogger vlog)
        {
            _logger = logger;
            _hub = hub;
            _http = http;
            _vlog = vlog;
        }

        // ===========================================================
        // POST /api/hook/event   ← recibe hooks reales del softphone
        // ===========================================================
        [HttpPost("event")]
        public async Task<IActionResult> ReceiveHook([FromBody] HookEventRequest hook)
        {
            if (hook == null)
                return BadRequest(new { error = "JSON inválido o vacío" });

            _logger.LogInformation("📩 HOOK recibido: {evt} | Agente={ag} | Usuario={u} | Servicio={svc}",
                hook.Evento, hook.Agente, hook.IdUsuario, hook.Servicio);

            // ===========================================================
            // 1) AUDITORÍA
            // ===========================================================
            string audit = VoiceLogger.Audit("HOOK/" + hook.Evento,
$@"Agente={hook.Agente}
Usuario={hook.IdUsuario}
Servicio={hook.Servicio}
Origen={hook.Origen}
Destino={hook.Destino}
Tipo={hook.Tipo}");

            _vlog.Hooks(audit);

            // ===========================================================
            // 2) Reenviar al navegador del agente
            // ===========================================================
            if (!string.IsNullOrWhiteSpace(hook.Agente))
            {
                string group = $"AGENTE_{hook.Agente}";
                await _hub.Clients.Group(group).SendAsync("hook", hook);

                _logger.LogInformation("📤 Hook reenviado por SignalR → {group}", group);
            }

            // ===========================================================
            // 3) Reenviar al CRM externo
            // ===========================================================
            try
            {
                var json = JsonSerializer.Serialize(hook);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = _http.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                // URL del CRM (puede moverse a appsettings.json)
                string crmUrl = "https://davinci.crm/hook/voiceapi";

                var res = await client.PostAsync(crmUrl, content);
                _logger.LogInformation("📡 Hook enviado al CRM: {code}", res.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error enviando hook al CRM");
                _vlog.Error("[CRM ERROR] " + ex.ToString());
            }

            return Ok(new { ok = true, recibido = hook.Evento });
        }
    }
}

