using VoiceAPI.Models.Agent;

namespace VoiceAPI.Models.Agent
{
    public class CreateAgentRequest
    {
        public string? PbxId { get; set; }
        public string? Cliente { get; set; }
        public string? IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Rol { get; set; }
        public string? IdAgente { get; set; }
        public string? Interno { get; set; }

        // JSON list
        public List<ServicioAgente>? Servicios { get; set; }
    }
}

