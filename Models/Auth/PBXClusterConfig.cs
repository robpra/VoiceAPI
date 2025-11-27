using System.Collections.Generic;

namespace VoiceAPI.Models.Auth
{
    public class PBXClusterConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public int AriPort { get; set; }
        public int AmiPort { get; set; }
        public int WssPort { get; set; }

        public string SipDomain { get; set; } = string.Empty;

        public string AriUser { get; set; } = string.Empty;
        public string AriPassword { get; set; } = string.Empty;

        // Lista de clientes (config general)
        public List<string> Clientes { get; set; } = new();

        // Cliente único (compatibilidad con tu AuthController actual)
        public string Cliente { get; set; } = string.Empty;

        public List<string> Servicios { get; set; } = new();

        public string ApiInternos { get; set; } = string.Empty;
    }
}

