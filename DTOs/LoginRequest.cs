namespace VoiceAPI.DTOs
{
    public class LoginRequest
    {
        public string? usuario { get; set; }
        public string? idUsuario { get; set; }
        public string? idAgente { get; set; }
        public string? servicio { get; set; }
        public string? cliente { get; set; }
        public string? rol { get; set; }
        public int? prioridad { get; set; }
        public string? extension { get; set; }
    }
}

