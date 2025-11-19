namespace VoiceAPI.DTOs
{
    public class LogoutRequest
    {
        public string idUsuario { get; set; } = "";
        public string idAgente { get; set; } = "";
        public string cliente { get; set; } = "";
        public string servicio { get; set; } = "";
        public string motivo { get; set; } = "";
    }
}

