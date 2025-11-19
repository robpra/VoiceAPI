namespace VoiceAPI.Models
{
    public class LoginRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string idUsuario { get; set; } = string.Empty;
        public string idAgente { get; set; } = string.Empty;
        public string servicio { get; set; } = string.Empty;
        public string cliente { get; set; } = string.Empty;
        public string prioridad { get; set; } = string.Empty;
        public string rol { get; set; } = string.Empty;
        public string? extension { get; set; }
    }
}

