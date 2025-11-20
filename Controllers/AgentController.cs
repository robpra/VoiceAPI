using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VoiceAPI.Data;
using VoiceAPI.Models;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentContext _db;
        private readonly IConfiguration _config;

        public AgentController(AgentContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ============================================================
        // Clase interna para PBXClusters
        // ============================================================
        private class PBXClusterConfig
        {
            public string Id { get; set; } = "";
            public List<string> Clientes { get; set; } = new();
        }

        // ============================================================
        // Obtener Cliente desde el primer cluster
        // ============================================================
        private string GetClienteFromClusters()
        {
            var clusters = _config.GetSection("PBXClusters").Get<List<PBXClusterConfig>>() 
                           ?? new List<PBXClusterConfig>();

            if (clusters.Count == 0) return "DESCONOCIDO";
            if (clusters[0].Clientes.Count == 0) return "DESCONOCIDO";

            return clusters[0].Clientes[0];
        }

        // ============================================================
        // Obtener PBXId según cliente
        // ============================================================
        private string GetPbxIdFromCliente(string cliente)
        {
            var clusters = _config.GetSection("PBXClusters").Get<List<PBXClusterConfig>>() 
                           ?? new List<PBXClusterConfig>();

            foreach (var cluster in clusters)
            {
                if (cluster.Clientes.Any(c => c.Equals(cliente, StringComparison.OrdinalIgnoreCase)))
                    return string.IsNullOrWhiteSpace(cluster.Id) ? "desconocido" : cluster.Id;
            }

            return "desconocido";
        }

        // ============================================================
        // POST: /api/agent/create
        // ============================================================
        [HttpPost("create")]
        public IActionResult CreateAgent([FromBody] AgentCreateRequest? req)
        {
            if (req == null)
                return BadRequest(new { error = "Solicitud inválida." });

            string rol = (req.rol ?? "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(req.idUsuario) ||
                string.IsNullOrWhiteSpace(req.nombre) ||
                string.IsNullOrWhiteSpace(req.apellido))
            {
                return BadRequest(new { error = "idUsuario, nombre y apellido son obligatorios." });
            }

            if (rol == "agente")
            {
                if (string.IsNullOrWhiteSpace(req.idAgente))
                    return BadRequest(new { error = "idAgente es obligatorio para agentes." });

                if (req.servicios == null || req.servicios.Count == 0)
                    return BadRequest(new { error = "Debe especificar servicios." });

                foreach (var s in req.servicios)
                {
                    if (string.IsNullOrWhiteSpace(s.IdServicio))
                        return BadRequest(new { error = "IdServicio es obligatorio." });
                }
            }
            else if (rol == "administrativo")
            {
                if (string.IsNullOrWhiteSpace(req.interno))
                    return BadRequest(new { error = "interno es obligatorio para administrativos." });
            }
            else
            {
                return BadRequest(new { error = "Rol inválido (agente / administrativo)" });
            }

            string cliente = GetClienteFromClusters();
            string pbxId = GetPbxIdFromCliente(cliente);

            string? serviciosJson = null;

            if (req.servicios != null)
            {
                var list = req.servicios.Select(s => new
                {
                    IdServicio = s.IdServicio,
                    Prioridad = s.Prioridad
                });

                serviciosJson = JsonSerializer.Serialize(list);
            }

            var usuario = new UsuarioTelefonia
            {
                PbxId = pbxId,
                Cliente = cliente,
                IdUsuario = req.idUsuario!,
                Nombre = req.nombre!,
                Apellido = req.apellido!,
                Rol = req.rol!,
                IdAgente = req.idAgente ?? "",
                Interno = rol == "administrativo" ? req.interno : null,
                Servicios = serviciosJson,
                FechaRegistro = DateTime.Now
            };

            _db.UsuariosTelefonia.Add(usuario);
            _db.SaveChanges();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario guardado correctamente.",
                idAgente = usuario.IdAgente,
                pbxId = usuario.PbxId
            });
        }

        // ============================================================
        // PUT: /api/agent/update/{idAgente}
        // ============================================================
        [HttpPut("update/{idAgente}")]
        public IActionResult UpdateAgent(string idAgente, [FromBody] AgentUpdateRequest? req)
        {
            if (string.IsNullOrWhiteSpace(idAgente))
                return BadRequest(new { error = "idAgente es obligatorio." });

            if (req == null)
                return BadRequest(new { error = "Solicitud inválida." });

            var usuario = _db.UsuariosTelefonia.FirstOrDefault(x => x.IdAgente == idAgente);
            if (usuario == null)
                return NotFound(new { error = "Agente no encontrado." });

            if (req.nombre != null) usuario.Nombre = req.nombre;
            if (req.apellido != null) usuario.Apellido = req.apellido;
            if (req.interno != null) usuario.Interno = req.interno;

            if (req.servicios != null)
            {
                var list = req.servicios.Select(s => new
                {
                    IdServicio = s.IdServicio,
                    Prioridad = s.Prioridad
                });

                usuario.Servicios = JsonSerializer.Serialize(list);
            }

            _db.SaveChanges();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario actualizado correctamente.",
                idAgente = usuario.IdAgente
            });
        }

        // ============================================================
        // POST: /api/agent/delete
        // ============================================================
        [HttpPost("delete")]
        public IActionResult DeleteAgent([FromBody] AgentDeleteRequest? req)
        {
            if (req == null)
                return BadRequest(new { error = "Solicitud inválida." });

            string idUsuario = req.idUsuario?.Trim() ?? "";
            string idAgente = req.idAgente?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(idUsuario) && string.IsNullOrWhiteSpace(idAgente))
                return BadRequest(new { error = "Debe enviar idUsuario o idAgente." });

            UsuarioTelefonia? target = null;

            if (!string.IsNullOrWhiteSpace(idUsuario))
                target = _db.UsuariosTelefonia.FirstOrDefault(x => x.IdUsuario == idUsuario);
            else
                target = _db.UsuariosTelefonia.FirstOrDefault(x => x.IdAgente == idAgente);

            if (target == null)
                return NotFound(new { error = "Usuario no encontrado." });

            _db.UsuariosTelefonia.Remove(target);
            _db.SaveChanges();

            return Ok(new
            {
                resultado = "OK",
                mensaje = "Usuario eliminado correctamente."
            });
        }
    }
}

