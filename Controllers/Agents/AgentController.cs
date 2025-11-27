using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VoiceAPI.Data;
using VoiceAPI.Hubs;
using VoiceAPI.Models.Agent;

namespace VoiceAPI.Controllers.Agents
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentContext _db;
        private readonly IHubContext<EventsHub> _hub;

        public AgentController(AgentContext db, IHubContext<EventsHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // ===========================================================
        // CREATE Agent
        // ===========================================================
        [HttpPost("create")]
        public IActionResult CreateAgent([FromBody] CreateAgentRequest req)
        {
            if (req == null)
                return BadRequest(new { error = "Request vacío" });

            if (string.IsNullOrWhiteSpace(req.IdUsuario))
                return BadRequest(new { error = "IdUsuario requerido" });

            if (string.IsNullOrWhiteSpace(req.IdAgente))
                return BadRequest(new { error = "IdAgente requerido" });

            if (_db.UsuariosTelefonia.Any(x => x.IdUsuario == req.IdUsuario))
                return Conflict(new { error = "Ya existe un usuario con ese IdUsuario" });

            var model = new UsuarioTelefonia
            {
                IdUsuario = req.IdUsuario,
                IdAgente = req.IdAgente,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Rol = req.Rol,
                Cliente = req.Cliente,
                PbxId = req.PbxId,
                Servicios = req.Servicios,
                Interno = req.Interno,
                FechaRegistro = DateTime.Now
            };

            _db.UsuariosTelefonia.Add(model);
            _db.SaveChanges();

            return Ok(model);
        }

        // ===========================================================
        // UPDATE Agent
        // ===========================================================
        [HttpPut("update")]
        public IActionResult UpdateAgent([FromBody] UpdateAgentRequest req)
        {
            var model = _db.UsuariosTelefonia
                .FirstOrDefault(x => x.IdUsuario == req.IdUsuario);

            if (model == null)
                return NotFound(new { error = "IdUsuario no encontrado" });

            model.Nombre = req.Nombre;
            model.Apellido = req.Apellido;
            model.Rol = req.Rol;
            model.Cliente = req.Cliente;
            model.PbxId = req.PbxId;
            model.Interno = req.Interno;
            model.Servicios = req.Servicios;

            _db.SaveChanges();

            return Ok(model);
        }

        // ===========================================================
        // DELETE Agent
        // ===========================================================
        [HttpDelete("delete/{idUsuario}")]
        public IActionResult DeleteAgent(string idUsuario)
        {
            var model = _db.UsuariosTelefonia
                .FirstOrDefault(x => x.IdUsuario == idUsuario);

            if (model == null)
                return NotFound(new { error = "IdUsuario no encontrado" });

            _db.UsuariosTelefonia.Remove(model);
            _db.SaveChanges();

            return Ok(new { ok = true, eliminado = idUsuario });
        }

        // ===========================================================
        // RELOGIN SIGNALR
        // ===========================================================
        [HttpPost("relogin")]
        public async Task<IActionResult> ReLogin([FromBody] AgentReLoginRequest req)
        {
            string groupName = $"AGENTE_{req.IdAgente}";

            await _hub.Clients.Group(groupName)
                .SendAsync("relogin", new
                {
                    idAgente = req.IdAgente,
                    mensaje = "Solicitar nuevo login desde CRM"
                });

            return Ok(new { ok = true, enviadoA = groupName });
        }
    }
}

