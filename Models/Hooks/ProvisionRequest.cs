namespace VoiceAPI.Models.Hooks
{
    public class ProvisionRequest
    {
        // Identificación del agente
        public string IdAgente { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string Usuario { get; set; } = "";

        // Servicio asociado
        public string Servicio { get; set; } = "";
        public string Rol { get; set; } = "";
        public int? Prioridad { get; set; }

        // Datos del cliente / PBX
        public string Cliente { get; set; } = "";
        public string Host { get; set; } = "";
        public string SipDomain { get; set; } = "";

        // WebSocket / PBX
        public string WssServer { get; set; } = "";
        public int WssPort { get; set; }

        // API para internos libres (ARI)
        public string ApiInternos { get; set; } = "";

        // Solo para administrativos (si aplica)
        public string? Extension { get; set; }
    }
}

