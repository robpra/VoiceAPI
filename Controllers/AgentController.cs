using Microsoft.AspNetCore.Mvc;
using VoiceAPI.Data;
using VoiceAPI.Models;
using VoiceAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using VoiceAPI.Hubs;


namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentDbContext _db;

        public AgentController(AgentDbContext db)
        {
            _db = db;
        }

        // ======================================
        // POST /api/agent/create
        // ======================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest req)
        {
            if (req == null)
                return BadRequest(new { error = "Cuerpo de request vacío" });

            // Validar Rol
            if (req.Rol != "agente" && req.Rol != "administrativo")
                return BadRequest(new { error = "Rol inválido (solo agente o administrativo)" });

            // Verificar si ya existe IdAgente
            var existente = await _db.UsuariosTelefonia
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAgente == req.IdAgente);

            if (existente != null)
                return Conflict(new { error = "Ya existe un agente con ese IdAgente" });

            var nuevo = new UsuarioTelefonia
            {
                PbxId = req.PbxId,
                Cliente = req.Cliente,
                IdUsuario = req.IdUsuario,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Rol = req.Rol,
                IdAgente = req.IdAgente,
                Interno = req.Interno,
                Servicios = req.Servicios,
                Prioridad = req.Prioridad,
                FechaRegistro = DateTime.UtcNow
            };

            _db.UsuariosTelefonia.Add(nuevo);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario creado correctamente",
                usuario = nuevo
            });
        }


        // ======================================
        // DELETE /api/agent/delete/{idAgente}
        // ======================================
        [HttpDelete("delete/{idAgente}")]
        public async Task<IActionResult> DeleteAgent(string idAgente)
        {
            if (string.IsNullOrWhiteSpace(idAgente))
                return BadRequest(new { error = "Debe especificar un IdAgente" });

            // Buscar agente por PK (IdAgente)
            var agente = await _db.UsuariosTelefonia.FindAsync(idAgente);

            if (agente == null)
                return NotFound(new { error = "No existe un agente con ese IdAgente" });

            _db.UsuariosTelefonia.Remove(agente);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Agente eliminado correctamente",
                IdAgente = idAgente
            });
        }

        // ======================================
        // PUT /api/agent/update/{idAgente}
        // ======================================
        [HttpPut("update/{idAgente}")]
        public async Task<IActionResult> UpdateAgent(string idAgente, [FromBody] UpdateAgentRequest req)
        {
            if (string.IsNullOrWhiteSpace(idAgente))
                return BadRequest(new { error = "Debe especificar un IdAgente" });

            var agente = await _db.UsuariosTelefonia.FindAsync(idAgente);

            if (agente == null)
                return NotFound(new { error = "No existe un agente con ese IdAgente" });

            // Validación de rol
            if (!string.IsNullOrWhiteSpace(req.Rol) &&
                req.Rol != "agente" &&
                req.Rol != "administrativo")
            {
                return BadRequest(new { error = "Rol inválido (debe ser agente o administrativo)" });
            }

            // APLICAR SOLO CAMPOS QUE LLEGAN
            if (req.PbxId != null) agente.PbxId = req.PbxId;
            if (req.Cliente != null) agente.Cliente = req.Cliente;
            if (req.IdUsuario != null) agente.IdUsuario = req.IdUsuario;
            if (req.Nombre != null) agente.Nombre = req.Nombre;
            if (req.Apellido != null) agente.Apellido = req.Apellido;
            if (req.Rol != null) agente.Rol = req.Rol;
            if (req.Interno != null) agente.Interno = req.Interno;
            if (req.Servicios != null) agente.Servicios = req.Servicios;
            if (req.Prioridad != null) agente.Prioridad = req.Prioridad;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Agente actualizado correctamente",
                agente
            });
        }


        // ======================================
        // POST /api/agent/relogin
        // ======================================
        [HttpPost("relogin")]
        public async Task<IActionResult> ReLogin([FromBody] ReLoginRequest req, [FromServices] IHubContext<EventsHub> hub)
        {
            if (req == null)
                return BadRequest(new { error = "Cuerpo vacío" });

            // Validar campos mínimos
            if (string.IsNullOrWhiteSpace(req.idAgente) ||
                string.IsNullOrWhiteSpace(req.idUsuario) ||
                string.IsNullOrWhiteSpace(req.NuevoServicio))
            {
                return BadRequest(new { error = "idAgente, idUsuario y NuevoServicio son obligatorios" });
            }

            // Buscar agente
            var agente = await _db.UsuariosTelefonia.FindAsync(req.idAgente);

            if (agente == null)
                return NotFound(new { error = "No existe un agente con ese IdAgente" });

            // Validar cliente
            if (!string.IsNullOrWhiteSpace(req.cliente) &&
                agente.Cliente?.ToUpper() != req.cliente.ToUpper())
            {
                return BadRequest(new { error = "El agente no pertenece a este cliente" });
            }

            // Guardar servicio anterior
            var servicioAnterior = agente.Servicios;

            // Actualizar datos
            agente.Servicios = req.NuevoServicio;
            agente.Prioridad = req.prioridad;
            agente.FechaRegistro = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // PREPARAR EVENTO SIGNALR
            var evento = new
            {
                time = DateTime.UtcNow,
                evento = "agentReLogin",
                agente = req.idAgente,
                usuario = req.idUsuario,
                cliente = req.cliente,
                servicioAnterior,
                servicioNuevo = req.NuevoServicio,
                prioridad = req.prioridad
            };

            // Enviar solo al agente
            await hub.Clients.Group($"agente:{req.idAgente}")
                .SendAsync("agentReLogin", evento);

            return Ok(new
            {
                resultado = "OK",
                mensaje = "ReLogin procesado correctamente",
                evento
            });
        }


// ======================================
// POST /api/agent/logout
// ======================================
[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] LogoutRequest req, [FromServices] IHubContext<EventsHub> hub)
{
    if (req == null)
        return BadRequest(new { error = "Cuerpo vacío" });

    if (string.IsNullOrWhiteSpace(req.idAgente))
        return BadRequest(new { error = "idAgente es obligatorio" });

    // Buscar agente en la base
    var agente = await _db.UsuariosTelefonia.FindAsync(req.idAgente);

    if (agente == null)
        return NotFound(new { error = "No existe un agente con ese IdAgente" });

    // Validar cliente (opcional)
    if (!string.IsNullOrWhiteSpace(req.cliente) &&
        agente.Cliente?.ToUpper() != req.cliente.ToUpper())
    {
        return BadRequest(new { error = "El agente no pertenece a este cliente" });
    }

    // Evento a enviar por SignalR
    var evento = new
    {
        time = DateTime.UtcNow,
        evento = "agentLogout",
        agente = req.idAgente,
        usuario = req.idUsuario,
        cliente = req.cliente,
        servicio = req.servicio,
        motivo = req.motivo
    };

    // Enviar SOLO al agente
    await hub.Clients.Group($"agente:{req.idAgente}")
        .SendAsync("agentLogout", evento);

    return Ok(new
    {
        resultado = "OK",
        mensaje = "Logout procesado correctamente",
        evento
    });
}







    }
}


