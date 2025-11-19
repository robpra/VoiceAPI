namespace VoiceAPI.DTOs
{
    public class ReLoginRequest
    {
        public string idUsuario { get; set; } = "";
        public string idAgente { get; set; } = "";
        public string ServicioActual { get; set; } = "";
        public string NuevoServicio { get; set; } = "";
        public string prioridad { get; set; } = "";
        public string cliente { get; set; } = "";
    }
}

