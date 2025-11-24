namespace VoiceAPI.Models.Auth
{
    public class PBXClusterConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int AriPort { get; set; }
        public int WssPort { get; set; }
        public string SipDomain { get; set; }
        public List<string> Clientes { get; set; }
        public List<string> Servicios { get; set; }
        public int AmiPort { get; set; }
        public string AriUser { get; set; }
        public string AriPassword { get; set; }
    }
}

