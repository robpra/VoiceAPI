using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using VoiceAPI.Data;
using VoiceAPI.Models;
using VoiceAPI.DTOs;
using VoiceAPI.Hubs;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AgentContext _db;
        private readonly IConfiguration _config;
        private readonly IHubContext<EventsHub> _hub;

        public AgentController(
            AgentContext db,
            IConfiguration config,
            IHubContext<EventsHub> hub)
        {
            _db = db;
            _config = config;
            _hub = hub;
        }

        // ============================================================
        // Clase interna PBX config (solo para PBXId y Cliente)
        // ============================================================
        private class PBXClusterConfig
        {
            public string Id { get; set; } = "";
            public List<string> Clientes { get; set; } = new();
        }

        // Obtener primer cliente definido
        private string GetClienteFromClusters()
        {
            var clusters = _config.GetSection("PBXClusters").Get<List<PBXClusterConfig>>() ?? new();
            return clusters.FirstOrDefault()?.Clientes?.FirstOrDefault() ?? "DESCONOCIDO";
        }

        private string GetPbxIdFromCliente(string cliente)
        {
            var clusters = _config.GetSection("PBXClusters").Get<List<PBXClusterConfig>>() ?? new();

            foreach (var c in clusters)
                if (c.Clientes.Contains(cliente, StringComparer.OrdinalIgnoreCase))
                    return c.Id;

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
            }
            else if (rol == "administrativo")
            {
                if (string.IsNullOrWhiteSpace(req.interno))
                    return BadRequest(new { error = "interno es obligatorio." });
            }
            else
            {
                return BadRequest(new { error = "Rol inválido." });
            }

            string cliente = GetClienteFromClusters();
            string pbxId   = GetPbxIdFromCliente(cliente);

            string? serviciosJson = null;

            if (req.servicios != null)
            {
                serviciosJson = JsonSerializer.Serialize(
                    req.servicios.Select(s => new {
                        IdServicio = s.IdServicio,
                        Prioridad = s.Prioridad
                    })
                );
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

            return Ok(new {
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
                usuario.Servicios = JsonSerializer.Serialize(
                    req.servicios.Select(s => new {
                        IdServicio = s.IdServicio,
                        Prioridad = s.Prioridad
                    })
                );
            }

            _db.SaveChanges();

            return Ok(new {
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
            string idAgente  = req.idAgente?.Trim()  ?? "";

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

            return Ok(new {
                resultado = "OK",
                mensaje = "Usuario eliminado correctamente."
            });
        }


        // ============================================================
        // POST: /api/agent/relogin  (SIN PBX / SIN COLAS)
        // ============================================================
        [HttpPost("relogin")]
        public async Task<IActionResult> ReLoginAgente([FromBody] ReLoginRequest req)
        {
            if (req == null)
                return BadRequest(new { resultado = "ERROR", mensaje = "Body inválido" });

            // 1) Validar agente existe
            var agente = _db.UsuariosTelefonia
                .FirstOrDefault(a =>
                    a.IdUsuario == req.idUsuario &&
                    a.IdAgente == req.idAgente);

            if (agente == null)
            {
                return NotFound(new {
                    resultado = "ERROR",
                    mensaje = "El agente no existe en BD"
                });
            }

            // 2) Construir payload evento SignalR
            var payload = new {
                evento = "agent.relogin",
                agente = req.idAgente,
                servicioAnterior = req.servicioActual,
                servicioNuevo = req.nuevoServicio,
                prioridad = req.prioridad,
                cliente = req.cliente
            };

            // 3) Emitir evento por SignalR
            await _hub.Clients.Group($"agente:{req.idAgente}")
                .SendAsync("agentReLogin", payload);

            // 4) Respuesta final
            return Ok(new {
                resultado = "OK",
                mensaje = "ReLogin notificado vía SignalR",
                idAgente = req.idAgente,
                servicioNuevo = req.nuevoServicio
            });
        }
    }
}

