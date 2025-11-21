namespace VoiceAPI.DTOs
{
    public class ReLoginRequest
    {
        public string idUsuario { get; set; } = "";
        public string idAgente { get; set; } = "";
        public string servicioActual { get; set; } = "";
        public string nuevoServicio { get; set; } = "";
        public int prioridad { get; set; }
        public string cliente { get; set; } = "";
    }
}

