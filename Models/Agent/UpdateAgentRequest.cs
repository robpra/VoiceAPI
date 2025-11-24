using VoiceAPI.Models.Agent;

namespace VoiceAPI.Models.Agent
{
    public class UpdateAgentRequest
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Interno { get; set; }

        // NOTA: servicios opcional, siempre guardado como JSON
        public List<ServicioAgente>? Servicios { get; set; }

        // idAgente opcional
        public string? IdAgente { get; set; }

        // rol podría cambiar, pero no es obligatorio
        public string? Rol { get; set; }
    }
}

