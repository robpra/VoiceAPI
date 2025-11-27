using System.Collections.Generic;

namespace VoiceAPI.Models.Agent
{
    public class CreateAgentRequest
    {
        public string pbxId { get; set; } = string.Empty;
        public string cliente { get; set; } = string.Empty;
        public string idUsuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; } = string.Empty;
        public string rol { get; set; } = string.Empty;

        // solo para rol = agente
        public string? idAgente { get; set; }
        public List<ServicioAgente>? servicios { get; set; }

        // solo para rol = administrativo
        public string? interno { get; set; }
    }
}

