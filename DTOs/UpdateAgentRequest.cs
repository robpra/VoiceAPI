using System.Collections.Generic;

namespace VoiceAPI.DTOs
{
    public class UpdateAgentRequest
    {
        public string? PbxId { get; set; }
        public string? Cliente { get; set; }
        public string? IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Rol { get; set; }

        public string? Interno { get; set; }

        public List<ServicioItem>? Servicios { get; set; }
    }
}

