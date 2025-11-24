namespace VoiceAPI.Models.Hooks
{
    public class HookEventRequest
    {
        public string evento { get; set; }

        // Datos del agente
        public string? idUsuario { get; set; }
        public string? usuario { get; set; }
        public string? agente { get; set; }
        public string? cliente { get; set; }
        public string? servicio { get; set; }

        // Datos de llamada (opcional)
        public string? tipo { get; set; }
        public string? origen { get; set; }
        public string? destino { get; set; }

        // Metadatos
        public string? timestamp { get; set; }
        public string uuid { get; set; }
        public int sequence { get; set; }
    }
}

