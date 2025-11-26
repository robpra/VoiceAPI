namespace VoiceAPI.Models.Provisioning
{
    public class ProvisionRequest
    {
        public string IdAgente { get; set; } = "";
        public string IdUsuario { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Rol { get; set; } = "";
        public string InstanceId { get; set; } = "";

        // Información de provisioning específica
        public string SipUser { get; set; } = "";
        public string SipPassword { get; set; } = "";
        public string SipDomain { get; set; } = "";
        public string WssUrl { get; set; } = "";
    }
}

