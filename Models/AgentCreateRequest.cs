using System.Collections.Generic;

namespace VoiceAPI.Models
{
    public class ServicioAsignado
    {
        public required string IdServicio { get; set; }
        public required int Prioridad { get; set; }
        public string? Interno { get; set; }
    }

    public class AgentCreateRequest
    {
        public required string idUsuario { get; set; }
        public required string nombre { get; set; }
        public required string apellido { get; set; }
        public required string rol { get; set; }

        // SOLO PARA AGENTE
        public string? idAgente { get; set; }
        public List<ServicioAsignado>? servicios { get; set; }

        // SOLO PARA ADMINISTRATIVO
        public string? interno { get; set; }
    }
}

