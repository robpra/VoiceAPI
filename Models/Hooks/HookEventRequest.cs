namespace VoiceAPI.Models.Hooks
{
    public class HookEventRequest
    {
        public string Evento { get; set; } = "";
        public string Agente { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Origen { get; set; } = "";
        public string Destino { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Uuid { get; set; } = "";
    }
}

