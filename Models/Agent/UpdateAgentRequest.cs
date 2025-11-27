namespace VoiceAPI.Models.Agent
{
    public class UpdateAgentRequest
    {
        public string IdUsuario { get; set; } = "";    // ← NECESARIO
        public string IdAgente { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Rol { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string PbxId { get; set; } = "";        // ← NECESARIO
        public string Interno { get; set; } = "";      // ← NECESARIO
        public List<ServicioAgente> Servicios { get; set; } = new();
    }
}

