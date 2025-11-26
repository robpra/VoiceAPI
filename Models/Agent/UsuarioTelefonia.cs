using System;
using System.Collections.Generic;

namespace VoiceAPI.Models.Agent
{
    public class UsuarioTelefonia
    {
        public int Id { get; set; }
        public string? PbxId { get; set; }
        public string? Cliente { get; set; }
        public string? IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Rol { get; set; }
        public string IdAgente { get; set; } = "";
        public string? Interno { get; set; }

        // ============================================================
        // AHORA Servicios ES UNA LISTA REAL → EF lo guarda como JSON
        // ============================================================
        public List<ServicioAgente> Servicios { get; set; } = new();

        public DateTime FechaRegistro { get; set; }
    }
}

