using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoiceAPI.Data;
using VoiceAPI.Models.Agent;
using VoiceAPI.Utils;
using System.Text.Json;

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

        // ============================================================
        // 1) CREATE AGENT
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest req)
        {
            // -------------------------------
            // VALIDACIÓN ROL
            // -------------------------------
            if (string.IsNullOrWhiteSpace(req.Rol))
            {
                return BadRequest(new { status = "error", message = "El campo rol es obligatorio." });
            }

            var rol = req.Rol.ToLower().Trim();

            // -------------------------------
            // VALIDACIÓN DATOS POR ROL
            // -------------------------------
            if (rol == "agente")
            {
                if (string.IsNullOrWhiteSpace(req.PbxId) ||
                    string.IsNullOrWhiteSpace(req.Cliente) ||
                    string.IsNullOrWhiteSpace(req.IdUsuario) ||
                    string.IsNullOrWhiteSpace(req.IdAgente) ||
                    req.Servicios == null)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Para rol 'agente' se requieren: pbxId, cliente, idUsuario, idAgente, servicios."
                    });
                }
            }
            else if (rol == "administrativo")
            {
                if (string.IsNullOrWhiteSpace(req.PbxId) ||
                    string.IsNullOrWhiteSpace(req.Cliente) ||
                    string.IsNullOrWhiteSpace(req.IdUsuario) ||
                    string.IsNullOrWhiteSpace(req.Nombre) ||
                    string.IsNullOrWhiteSpace(req.Interno))
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Para rol 'administrativo' se requieren: pbxId, cliente, idUsuario, nombre, interno."
                    });
                }
            }
            else
            {
                return BadRequest(new { status = "error", message = "Rol inválido." });
            }

            // -------------------------------
            // EXISTE USUARIO?
            // -------------------------------
            bool existe = await _db.UsuariosTelefonia
                .AnyAsync(u => u.IdUsuario == req.IdUsuario);

            if (existe)
            {
                return Conflict(new
                {
                    status = "error",
                    message = "idUsuario ya existe en el sistema."
                });
            }

            // -------------------------------
            // PREPARAR SERVICIOS (JSON)
            // -------------------------------
            string serviciosJson = "[]";
            if (req.Servicios != null)
                serviciosJson = ServicioHelper.ToJson(req.Servicios);

            // -------------------------------
            // CREAR OBJETO DB
            // -------------------------------
            var nuevo = new UsuarioTelefonia
            {
                PbxId = req.PbxId,
                Cliente = req.Cliente,
                IdUsuario = req.IdUsuario,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Rol = rol,
                IdAgente = req.IdAgente ?? "",
                Interno = req.Interno,
                Servicios = serviciosJson,
                FechaRegistro = DateTime.Now
            };

            _db.UsuariosTelefonia.Add(nuevo);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                status = "ok",
                message = "Agente creado correctamente.",
                id = nuevo.Id
            });
        }

        // ============================================================
        // 2) UPDATE AGENT
        // ============================================================
        [HttpPut("update/{idUsuario}")]
        public async Task<IActionResult> UpdateAgent(string idUsuario, [FromBody] UpdateAgentRequest req)
        {
            var agente = await _db.UsuariosTelefonia
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (agente == null)
            {
                return NotFound(new { status = "error", message = "Usuario no encontrado." });
            }

            // -------------------------------
            // APLICAR CAMPOS OPCIONALES
            // -------------------------------
            if (req.Nombre != null) agente.Nombre = req.Nombre;
            if (req.Apellido != null) agente.Apellido = req.Apellido;
            if (req.Interno != null) agente.Interno = req.Interno;
            if (req.IdAgente != null) agente.IdAgente = req.IdAgente;

            // Rol puede cambiar
            if (req.Rol != null) agente.Rol = req.Rol;

            // Servicios → JSON siempre válido
            if (req.Servicios != null)
            {
                agente.Servicios = ServicioHelper.ToJson(req.Servicios);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                status = "ok",
                message = "Agente actualizado."
            });
        }

        // ============================================================
        // 3) DELETE AGENT
        // ============================================================
        [HttpDelete("delete/{idUsuario}")]
        public async Task<IActionResult> DeleteAgent(string idUsuario)
        {
            var agente = await _db.UsuariosTelefonia
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (agente == null)
            {
                return NotFound(new { status = "error", message = "Usuario no encontrado." });
            }

            _db.UsuariosTelefonia.Remove(agente);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                status = "ok",
                message = "Agente eliminado correctamente."
            });
        }
    }
}

