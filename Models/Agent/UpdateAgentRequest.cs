namespace VoiceAPI.Models.Agent
{
    public class UpdateAgentRequest
    {
        public string IdUsuario { get; set; } = string.Empty;
        public string IdAgente { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string PbxId { get; set; } = string.Empty;

        public List<ServicioAgente> Servicios { get; set; } = new();
    }
}

