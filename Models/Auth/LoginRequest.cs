namespace VoiceAPI.Models.Auth
{
    public class LoginRequest
    {
        /// <summary>
        /// Usuario visible / nombre en Davinci (opcional)
        /// </summary>
        public string? Usuario { get; set; }

        /// <summary>
        /// ID único en el CRM/Davinci
        /// </summary>
        public string IdUsuario { get; set; } = string.Empty;

        /// <summary>
        /// ID único del agente dentro del CRM
        /// </summary>
        public string IdAgente { get; set; } = string.Empty;

        /// <summary>
        /// ID del servicio al que intenta loguearse.
        /// Clave para mapear PBX y Cliente.
        /// </summary>
        public string Servicio { get; set; } = string.Empty;

        /// <summary>
        /// Rol: "agente" | "administrativo"
        /// Soporta también: "0" (agente), "1" (administrativo)
        /// </summary>
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Prioridad del agente dentro del servicio (opcional)
        /// </summary>
        public int? Prioridad { get; set; }

        /// <summary>
        /// Extensión fija SOLO para administrativos.
        /// Para agentes = ignorado
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// Cliente (opcional)
        /// — se obtiene automáticamente desde PBXClusters
        /// </summary>
        public string? Cliente { get; set; }

        /// <summary>
        /// InstanceId generado por el softphone (index.html / signalr-client.js)
        /// Necesario para vincular login → SignalR
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;
    }
}

