using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VoiceAPI.Models;
using VoiceAPI.Services;
using VoiceAPI.Hubs;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PBXManager _pbx;
        private readonly JwtService _jwt;
        private readonly ExtensionAllocator _ext;
        private readonly IHubContext<EventsHub> _hub;

        public AuthController(
            PBXManager pbx,
            JwtService jwt,
            ExtensionAllocator ext,
            IHubContext<EventsHub> hub)
        {
            _pbx = pbx;
            _jwt = jwt;
            _ext = ext;
            _hub = hub;
        }

        /* ============================================================
           LOGIN DEL AGENTE
           ============================================================ */
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // Validar PBX según cliente
            var pbx = _pbx.GetPBXByCliente(req.cliente);
            if (pbx == null)
                return BadRequest($"No existe PBX configurada para el cliente: {req.cliente}");

            // Obtener extensión libre (si no viene una fija del CRM)
            string? extension = req.extension;

            if (extension == null)
            {
                extension = await _ext.GetFreeExtension(pbx);
                if (extension == null)
                    return StatusCode(500, "No hay extensiones SIP libres disponibles.");
            }

            string sipPassword = $"{extension}PSW";

            // Crear payload para el JWT
            var payload = new Dictionary<string, string>
            {
                { "usuario", req.usuario },
                { "idUsuario", req.idUsuario },
                { "idAgente", req.idAgente },
                { "cliente", req.cliente },
                { "servicio", req.servicio },
                { "prioridad", req.prioridad },
                { "rol", req.rol },
                { "extension", extension },
                { "pbxId", pbx["Id"]! },
                { "pbxHost", pbx["Host"]! }
            };

            string jwt = _jwt.GenerateToken(payload);

            /* ============================================================
               NOTIFICAR LOGIN SOLO A LOS GRUPOS RELEVANTES
               ============================================================ */

            var loginEvent = new
            {
            
    		// TIMING
    		time = DateTime.UtcNow,
    		timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    		//sessionId = sessionId,

    		// DATOS CRM
    		usuario = req.usuario,
    		idUsuario = req.idUsuario,
    		crmName = req.usuario,
    		crmId = req.idUsuario,

    		// AGENTE
    		agente = req.idAgente,
    		cliente = req.cliente,
    		servicio = req.servicio,
    		prioridad = req.prioridad,
    		rol = req.rol,
    		grupoAtencion = req.servicio,  // alias lógico

    		// SIP / WEBRTC
    		extension = extension,
   		SipUsername = extension,
    		SipPassword = $"{extension}PSW",
    		SipDomain = pbx["SipDomain"],
   		 WssServer = pbx["Host"],
    		WssPort = pbx["WssPort"],

    		// PBX INFO
    		pbxId = pbx["Id"],
    		pbxHost = pbx["Host"],

    		// METADATA
   		// ipCliente = ipCliente,
    		estado = "logueado"



	    };

            // Grupo por agente

		// Enviar a PRELOGIN → todos los softphones reciben el primer evento
		await _hub.Clients.Group("prelogin")
    			.SendAsync("agentLogin", loginEvent);

		// Luego ya solo al agente
		await _hub.Clients.Group($"agente:{req.idAgente}")
    			.SendAsync("agentLogin", loginEvent);




            /* ============================================================
               RESPUESTA AL CRM / SOFTPHONE WEBRTC
               ============================================================ */

            return Ok(new
            {
                resultado = "OK",
                jwt,
                webrtc = new
                {
                    SipDomain = pbx["SipDomain"],
                    WssServer = pbx["Host"],
                    WebSocketPort = pbx["WssPort"],
                    SipUsername = extension,
                    SipPassword = sipPassword,
                    extension
                },
                pbx = new
                {
                    id = pbx["Id"],
                    host = pbx["Host"]
                }
            });
        }
    }
}

