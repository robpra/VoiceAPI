using Microsoft.AspNetCore.Mvc;
using VoiceAPI.Data;
using VoiceAPI.Models.Agent;

namespace VoiceAPI.Models.Agent
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentContext _db;

        public AgentController(AgentContext db)
        {
            _db = db;
        }

        // ===========================================================
        // POST /api/agent/create
        // ===========================================================
        [HttpPost("create")]
        public IActionResult CreateAgent([FromBody] CreateAgentRequest req)
        {
            var model = new UsuarioTelefonia
            {
                IdUsuario = req.IdUsuario,
                IdAgente = req.IdAgente,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Rol = req.Rol,
                Cliente = req.Cliente,
                PbxId = req.PbxId,

                // Guardamos la lista como List<ServicioAgente>
                Servicios = req.Servicios
            };

            _db.UsuariosTelefonia.Add(model);
            _db.SaveChanges();

            return Ok(model);
        }

        // ===========================================================
        // PUT /api/agent/update
        // ===========================================================
        [HttpPut("update")]
        public IActionResult UpdateAgent([FromBody] UpdateAgentRequest req)
        {
            var existing = _db.UsuariosTelefonia.FirstOrDefault(x => x.IdUsuario == req.IdUsuario);

            if (existing == null)
                return NotFound(new { error = "Agente no encontrado" });

            // Actualización de datos
            existing.Nombre = req.Nombre;
            existing.Apellido = req.Apellido;
            existing.Rol = req.Rol;
            existing.Cliente = req.Cliente;
            existing.PbxId = req.PbxId;

            // Guardamos la nueva lista de servicios
            existing.Servicios = req.Servicios;

            _db.SaveChanges();

            return Ok(existing);
        }
    }
}

