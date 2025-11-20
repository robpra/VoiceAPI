using System.Collections.Generic;

namespace VoiceAPI.DTOs
{
    public class CreateAgentRequest
    {
        public string? PbxId { get; set; }
        public string Cliente { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Rol { get; set; } = "";  // agente | administrativo

        public string? IdAgente { get; set; }
        public string? Interno { get; set; }

        // Nueva estructura JSON
        public List<ServicioItem>? Servicios { get; set; }
    }
}

