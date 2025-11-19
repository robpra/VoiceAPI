using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VoiceAPI.Services
{
    public class AriClient
    {
        private readonly HttpClient _http;

        public AriClient(IConfiguration config)
        {
            _http = new HttpClient();
        }

        private void SetAuth(string user, string pass)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        public async Task<JsonDocument?> GetSipEndpoints(string host, int port, string user, string pass)
        {
            SetAuth(user, pass);
            var url = $"http://{host}:{port}/ari/endpoints/SIP";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
                return null;

            return JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        }

        public async Task<JsonDocument?> GetChannels(string host, int port, string user, string pass)
        {
            SetAuth(user, pass);
            var url = $"http://{host}:{port}/ari/channels";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
                return null;

            return JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        }
    }
}

