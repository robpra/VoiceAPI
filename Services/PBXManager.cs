using Microsoft.Extensions.Configuration;

namespace VoiceAPI.Services
{
    public class PBXManager
    {
        private readonly IConfiguration _config;

        public PBXManager(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<IConfigurationSection> GetAllPBX()
        {
            return _config.GetSection("PBXClusters").GetChildren();
        }

        public IConfigurationSection? GetPBXByCliente(string cliente)
        {
            var clusters = GetAllPBX();

            foreach (var pbx in clusters)
            {
                var clientes = pbx.GetSection("Clientes").Get<string[]>();

                if (clientes != null &&
                    clientes.Contains(cliente, StringComparer.OrdinalIgnoreCase))
                {
                    return pbx;
                }
            }

            return null;
        }

        public string PingPBXs()
        {
            var clusters = GetAllPBX().ToList();

            if (!clusters.Any())
                return "No hay PBXs configuradas";

            return string.Join(" | ", clusters.Select(p =>
                $"{p["Name"]} → {p["Host"]}"
            ));
        }
    }
}

