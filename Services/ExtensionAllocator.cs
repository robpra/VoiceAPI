using System.Text.Json;

namespace VoiceAPI.Services
{
    public class ExtensionAllocator
    {
        private readonly AriClient _ari;

        public ExtensionAllocator(AriClient ari)
        {
            _ari = ari;
        }

        public async Task<string?> GetFreeExtension(IConfigurationSection pbx)
        {
            string host = pbx["Host"]!;
            int port = int.Parse(pbx["AriPort"]!);
            string user = pbx["AriUser"]!;
            string pass = pbx["AriPassword"]!;

            var endpoints = await _ari.GetSipEndpoints(host, port, user, pass);
            if (endpoints == null)
                return null;

            foreach (var ep in endpoints.RootElement.EnumerateArray())
            {
                string ext = ep.GetProperty("resource").GetString()!;
                int channels = ep.GetProperty("channel_ids").GetArrayLength();

                if (channels == 0)
                    return ext;
            }

            return null;
        }
    }
}

