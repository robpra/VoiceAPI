using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoiceAPI.Utils
{
    public class ServicioHelper
    {
        // =====================================
        // MODELO PBX desde appsettings.json
        // =====================================
        public class PBXClusterItem
        {
            public string Id { get; set; } = "";
            public string Host { get; set; } = "";
            public int WssPort { get; set; }
            public int AriPort { get; set; }
            public string SipDomain { get; set; } = "";
            public List<string> Clientes { get; set; } = new();
            public List<string> Servicios { get; set; } = new();
        }

        // Estructura de servicios usados por agentes (DB)
        public class ServicioAgenteItem
        {
            public string Servicio { get; set; } = "";
            public int Penalty { get; set; } = 0;
        }


        // Lista de PBXs cargadas desde appsettings.json
        private readonly List<PBXClusterItem> _pbxs;


        // ===========================
        // Constructor
        // ===========================
        public ServicioHelper(IConfiguration config)
        {
            _pbxs = config.GetSection("PBXClusters").Get<List<PBXClusterItem>>() ?? new();

            Console.WriteLine($"ServicioHelper inicializado con {_pbxs.Count} PBXs.");
        }


        // ===========================
        // Obtener PBX por número de servicio
        // ===========================
        public PBXClusterItem? GetClusterByServicio(string servicio)
        {
            return _pbxs.FirstOrDefault(x =>
                x.Servicios.Any(s => s.Equals(servicio, StringComparison.OrdinalIgnoreCase)));
        }


        // ===========================
        // Obtener Cliente por número de servicio
        // ===========================
        public string GetClienteByServicio(string servicio)
        {
            var pbx = GetClusterByServicio(servicio);
            if (pbx == null) return "";
            return pbx.Clientes.FirstOrDefault() ?? "";
        }


        // ===========================
        // NUEVO: Obtener PBX + Cliente
        // ===========================
        public (PBXClusterItem? pbx, string cliente) GetClusterAndCliente(string servicio)
        {
            var pbx = GetClusterByServicio(servicio);
            if (pbx == null)
                return (null, "");

            var cliente = GetClienteByServicio(servicio);
            return (pbx, cliente);
        }


        // ===========================================================
        // Conversión para guardado en DB: Lista <→ JSON STRING
        // ===========================================================

        public string ServiciosToJson(List<ServicioAgenteItem> servicios)
        {
            if (servicios == null) return "[]";
            return JsonConvert.SerializeObject(servicios);
        }

        public List<ServicioAgenteItem> ServiciosFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<ServicioAgenteItem>();

            try
            {
                return JsonConvert.DeserializeObject<List<ServicioAgenteItem>>(json)
                       ?? new List<ServicioAgenteItem>();
            }
            catch
            {
                return new List<ServicioAgenteItem>();
            }
        }
    }
}

