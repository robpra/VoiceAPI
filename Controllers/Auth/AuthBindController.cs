using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VoiceAPI.Hubs;

namespace VoiceAPI.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthBindController : ControllerBase
    {
        //private static readonly Dictionary<string, string> AgentInstanceMap = new();
	internal static readonly Dictionary<string, string> AgentInstanceMap = new();
        private readonly IHubContext<EventsHub> _hubContext;

        public AuthBindController(IHubContext<EventsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public class BindRequest
        {
            public string? IdAgente { get; set; }
            public string? InstanceId { get; set; }
        }

        /// <summary>
        /// Recibe el ID de agente y la instancia del softphone para enlazar la sesión.
        /// </summary>
        [HttpPost("bind")]
        public IActionResult Bind([FromBody] BindRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.IdAgente) ||
                string.IsNullOrWhiteSpace(req.InstanceId))
            {
                return BadRequest(new { error = "Campos requeridos: IdAgente, InstanceId" });
            }

            lock (AgentInstanceMap)
            {
                AgentInstanceMap[req.IdAgente] = req.InstanceId;
            }

            Console.WriteLine("────────────────────────────────────────────");
            Console.WriteLine("🔗 BINDING REGISTRADO");
            Console.WriteLine($"   → Agente     : {req.IdAgente}");
            Console.WriteLine($"   → InstanceID : {req.InstanceId}");
            Console.WriteLine("────────────────────────────────────────────");

            return Ok(new
            {
                status = "ok",
                idAgente = req.IdAgente,
                instance = req.InstanceId
            });
        }

        /// <summary>
        /// Obtiene la instancia asociada a un agente.
        /// </summary>
        [HttpGet("instance/{idAgente}")]
        public IActionResult GetInstance(string idAgente)
        {
            lock (AgentInstanceMap)
            {
                if (AgentInstanceMap.TryGetValue(idAgente, out var instanceId))
                {
                    return Ok(new { idAgente, instanceId });
                }
            }

            return NotFound(new { error = "Agente no encontrado o sin instancia asociada." });
        }

        /// <summary>
        /// Envía un evento manualmente a la instancia de un agente (para pruebas).
        /// </summary>
        [HttpPost("send-test")]
        public async Task<IActionResult> SendTest([FromBody] BindRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.IdAgente))
                return BadRequest(new { error = "Falta idAgente" });

            string? instanceId;

            lock (AgentInstanceMap)
            {
                if (!AgentInstanceMap.TryGetValue(req.IdAgente, out instanceId))
                    return BadRequest(new { error = "No existe binding para este agente." });
            }

            var grupo = $"instancia:{instanceId}";

            Console.WriteLine($"➡ Enviando mensaje de prueba a grupo {grupo}");

            await _hubContext.Clients.Group(grupo)
                .SendAsync("test", new { msg = "Hola desde VoiceAPI", agente = req.IdAgente });

            return Ok(new { status = "ok", sentTo = grupo });
        }
    }
}

