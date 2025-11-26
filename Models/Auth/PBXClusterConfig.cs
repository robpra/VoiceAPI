using System.Collections.Generic;

namespace VoiceAPI.Models.Auth
{
    public class PBXClusterConfig
    {
        public List<PBXCluster> PBXClusters { get; set; } = new();
    }

    public class PBXCluster
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;

        public int AriPort { get; set; }
        public int WssPort { get; set; }
        public int AmiPort { get; set; }

        public string SipDomain { get; set; } = string.Empty;

        public string AriUser { get; set; } = string.Empty;
        public string AriPassword { get; set; } = string.Empty;

        public List<string> Clientes { get; set; } = new();
        public List<string> Servicios { get; set; } = new();
    }
}

