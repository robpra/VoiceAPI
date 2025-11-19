using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Collections.Concurrent;

[ApiController]
[Route("api/[controller]")]
public class HookController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<HookController> _logger;
    private static readonly HttpClient _http = new();

    // Secuencia por callId
    private static readonly ConcurrentDictionary<string, long> CallSequence =
        new ConcurrentDictionary<string, long>();

    public HookController(IConfiguration config, ILogger<HookController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpPost("event")]
    public async Task<IActionResult> ReceiveHook([FromBody] JObject data)
    {
        if (data == null)
        {
            _logger.LogWarning("❌ JSON vacío o inválido recibido");
            return BadRequest(new { error = "JSON vacío o inválido" });
        }

        _logger.LogInformation("📥 Hook recibido: {json}", data.ToString());

        // 1️⃣ Timestamp y UUID
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string uuid = Guid.NewGuid().ToString();

        // 2️⃣ callId → obligatorio
        string callId = data["callId"]?.ToString() ?? "NO-CALLID";

        long sequence = CallSequence.AddOrUpdate(
            callId,
            1,
            (key, oldValue) => oldValue + 1
        );

        // 3️⃣ Enriquecer el JSON
        data["timestamp"] = timestamp;
        data["uuid"] = uuid;
        data["sequence"] = sequence;

        _logger.LogInformation("⚙️ Evento enriquecido: {json}", data.ToString());

        // 4️⃣ Destino del hook
        string destino = _config["Hooks:Destino"] ?? "";
        if (string.IsNullOrWhiteSpace(destino))
        {
            _logger.LogError("❌ Destino no configurado en appsettings.json");
            return StatusCode(500, new { error = "Destino no configurado en appsettings.json" });
        }

        _logger.LogInformation("📤 Reenviando hook a: {destino}", destino);

        try
        {
            var jsonString = data.ToString();
            var content = new StringContent(jsonString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // 5️⃣ Reenviar
            HttpResponseMessage response = await _http.PostAsync(destino, content);

            int status = (int)response.StatusCode;

            _logger.LogInformation("📨 Respuesta CRM/PHP HTTP {status}", status);

            // 6️⃣ Log interno
            await System.IO.File.AppendAllTextAsync(
                // "/root/VoiceAPI/hook_debug.log",
		"/var/www/html/hookVoiceAPI/hook_log.txt",
                $"{timestamp} | callId={callId} | seq={sequence} | Enviado: {jsonString}\n"
            );

            return Ok(new
            {
                resultado = "OK",
                reenviadoA = destino,
                status,
                sequence,
                callId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Error reenviando datos: {msg}", ex.Message);

            return StatusCode(500, new
            {
                error = "Error reenviando datos",
                detalle = ex.Message
            });
        }
    }
}

