using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Text;
using VoiceAPI.Hubs;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<EventsHub> _hub;
        private readonly HttpClient _http;

        public HookController(IConfiguration config, IHubContext<EventsHub> hub)
        {
            _config = config;
            _hub = hub;
            _http = new HttpClient();
        }

        [HttpPost("event")]
        public async Task<IActionResult> ReceiveHook([FromBody] object rawPayload)
        {
            // Convertimos raw JSON a string
            string json = rawPayload.ToString();

            // Extraemos el evento de forma segura
            dynamic root = JsonConvert.DeserializeObject<dynamic>(json);
            string evento = root.evento != null ? (string)root.evento : "undefined";

            string destino = _config["Hooks:Destino"];
            if (string.IsNullOrWhiteSpace(destino))
                return StatusCode(500, new { status = "error", message = "Hooks.Destino no configurado." });

            // --------------------------------------------------------
            // LOG LOCAL
            // --------------------------------------------------------
            try
            {
                Directory.CreateDirectory("/var/log/voiceapi");

                var logLine =
                    $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {evento} | {json}";

                await System.IO.File.AppendAllTextAsync(
                    "/var/log/voiceapi/hooks.log",
                    logLine + "\n"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error escribiendo log local: {ex.Message}");
            }

            // --------------------------------------------------------
            // 1) Reenviar RAW a Davinci/PHP
            // --------------------------------------------------------
            HttpResponseMessage resp;
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                resp = await _http.PostAsync(destino, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reenviando hook: {ex.Message}");
                return StatusCode(500, new { status = "error", message = "Error reenviando hook" });
            }

            // --------------------------------------------------------
            // 2) Emitir por SignalR como JSON STRING (no dynamic)
            // --------------------------------------------------------
            await _hub.Clients.Group("agentes")
                .SendAsync("hookEvent", json);

            // --------------------------------------------------------
            // 3) Respuesta final al softphone
            // --------------------------------------------------------
            return Ok(new
            {
                status = "ok",
                forwardedTo = destino,
                responseCode = (int)resp.StatusCode
            });
        }
    }
}

