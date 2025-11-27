using Microsoft.Extensions.Configuration;
using VoiceAPI.Models.Auth;

namespace VoiceAPI.Utils
{
    public static class ServicioHelper
    {
        private static IConfiguration? _config;

        public static void Initialize(IConfiguration config)
        {
            _config = config;
        }

        // NUEVO → Match real basado en Servicio (tu lógica vieja)
        public static (PBXClusterConfig? pbx, string cliente) GetClusterByService(string servicio)
        {
            if (_config == null)
                return (null, "");

            var clusters = _config.GetSection("PBXClusters")
                .Get<List<PBXClusterConfig>>() ?? new();

            servicio = servicio?.Trim() ?? "";

            foreach (var c in clusters)
            {
                if (c.Servicios.Contains(servicio))
                {
                    string cli = c.Clientes.FirstOrDefault() ?? "";
                    return (c, cli);
                }
            }

            return (null, "");
        }

        // Si en el futuro querés usar Cliente+Servicio:
        public static PBXClusterConfig? GetPBX(string cliente, string servicio)
        {
            if (_config == null)
                return null;

            var clusters = _config.GetSection("PBXClusters")
                .Get<List<PBXClusterConfig>>() ?? new();

            cliente = cliente?.Trim().ToUpper() ?? "";
            servicio = servicio?.Trim() ?? "";

            var match = clusters.FirstOrDefault(c =>
                c.Clientes.Any(x => x.Trim().ToUpper() == cliente) &&
                c.Servicios.Any(s => s.Trim() == servicio));

            if (match != null)
                return match;

            return clusters.FirstOrDefault(c =>
                c.Servicios.Any(s => s.Trim() == servicio));
        }
    }
}

