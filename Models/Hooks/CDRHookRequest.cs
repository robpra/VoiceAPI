namespace VoiceAPI.Models.Hooks
{
    public class CDRHookRequest
    {
        public string evento { get; set; }
        public string cliente { get; set; }
        public string idAgente { get; set; }
        public string idServicio { get; set; }
        public string tipo { get; set; }
        public string numero { get; set; }
        public int duracion { get; set; }
        public string motivo { get; set; }
        public string uniqueid { get; set; }
        public string grabacion { get; set; }
        public string timestamp { get; set; }
    }
}

