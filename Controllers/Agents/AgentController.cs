using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VoiceAPI.Data;
using VoiceAPI.Models.Agent;

namespace VoiceAPI.Controllers.Agents
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

        // =====================================================================
        //  CREATE AGENTE / ADMINISTRATIVO
        // =====================================================================
        [HttpPost("create")]
        public IActionResult CreateAgent([FromBody] CreateAgentRequest req)
        {
            if (req == null)
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = "Body inválido"
                });
            }

            // --------------------------------------------------------
            // Validación general
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(req.pbxId))
                return BadRequest(new { resultado = "ERROR", mensaje = "pbxId es requerido" });

            if (string.IsNullOrWhiteSpace(req.cliente))
                return BadRequest(new { resultado = "ERROR", mensaje = "cliente es requerido" });

            if (string.IsNullOrWhiteSpace(req.idUsuario))
                return BadRequest(new { resultado = "ERROR", mensaje = "idUsuario es requerido" });

            if (string.IsNullOrWhiteSpace(req.nombre))
                return BadRequest(new { resultado = "ERROR", mensaje = "nombre es requerido" });

            if (string.IsNullOrWhiteSpace(req.rol))
                return BadRequest(new { resultado = "ERROR", mensaje = "rol es requerido" });

            var rol = req.rol.ToLower().Trim();

            // --------------------------------------------------------
            // Validaciones según ROL
            // --------------------------------------------------------
            if (rol == "agente")
            {
                if (string.IsNullOrWhiteSpace(req.idAgente))
                    return BadRequest(new { resultado = "ERROR", mensaje = "idAgente es requerido para agentes" });

                if (req.servicios == null || req.servicios.Count == 0)
                    return BadRequest(new { resultado = "ERROR", mensaje = "Debe enviar servicios para el rol agente" });

                // prioridad default
                foreach (var s in req.servicios)
                {
                    if (s.prioridad == null)
                        s.prioridad = 1;
                }
            }
            else if (rol == "administrativo")
            {
                if (string.IsNullOrWhiteSpace(req.interno))
                    return BadRequest(new { resultado = "ERROR", mensaje = "interno es requerido para administrativos" });

                req.idAgente = null;
                req.servicios = null;
            }
            else
            {
                return BadRequest(new { resultado = "ERROR", mensaje = "rol inválido (solo agente / administrativo)" });
            }

            // --------------------------------------------------------
            // Validar que no exista
            // --------------------------------------------------------
            if (_db.UsuariosTelefonia.Any(u =>
                u.IdUsuario == req.idUsuario &&
                u.Cliente == req.cliente))
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = "El idUsuario ya existe para este cliente"
                });
            }

            // --------------------------------------------------------
            // Crear objeto UsuarioTelefonia
            // --------------------------------------------------------
            var nuevo = new UsuarioTelefonia
            {
                PbxId = req.pbxId,
                Cliente = req.cliente,
                IdUsuario = req.idUsuario,
                Nombre = req.nombre,
                Apellido = req.apellido,
                Rol = rol,
                FechaRegistro = DateTime.UtcNow,
                IdAgente = rol == "agente" ? req.idAgente : null,
                Interno = rol == "administrativo" ? req.interno : "",
                Servicios = rol == "agente"
                    ? JsonConvert.SerializeObject(req.servicios)
                    : null
            };

            _db.UsuariosTelefonia.Add(nuevo);
            _db.SaveChanges();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario creado correctamente",
                idUsuario = nuevo.IdUsuario
            });
        }

        // =====================================================================
        //  DELETE USUARIO
        // =====================================================================
        [HttpDelete("delete")]
        public IActionResult DeleteAgent([FromBody] DeleteAgentRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.IdUsuario))
            {
                return BadRequest(new
                {
                    resultado = "ERROR",
                    mensaje = "Debe indicar idUsuario"
                });
            }

            var usuario = _db.UsuariosTelefonia
                .FirstOrDefault(u =>
                    u.IdUsuario == req.IdUsuario &&
                    u.Cliente == req.Cliente);

            if (usuario == null)
            {
                return NotFound(new
                {
                    resultado = "ERROR",
                    mensaje = "Usuario no encontrado"
                });
            }

            _db.UsuariosTelefonia.Remove(usuario);
            _db.SaveChanges();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario eliminado correctamente",
                idUsuario = req.IdUsuario
            });
        }
    }
}

