using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace VoiceAPI.Hubs
{
    public class EventsHub : Hub
    {
        private readonly ILogger<EventsHub> _logger;

        // Tabla de instancias activas → instanceId → connectionId
        private static readonly ConcurrentDictionary<string, string> _instancias =
            new ConcurrentDictionary<string, string>();

        public EventsHub(ILogger<EventsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            string instanceId = Context.GetHttpContext()?.Request.Query["instanceId"];

            _logger.LogInformation("────────────────────────────────────────────");
            _logger.LogInformation("🔌 Nueva conexión SignalR");
            _logger.LogInformation("→ ConnectionId : {0}", connectionId);
            _logger.LogInformation("→ instanceId   : {0}", instanceId);

            if (!string.IsNullOrEmpty(instanceId))
            {
                _instancias[instanceId] = connectionId;

                string group = $"instancia:{instanceId}";
                await Groups.AddToGroupAsync(connectionId, group);
                _logger.LogInformation("✔ Unido al grupo: {0}", group);
            }

            await Groups.AddToGroupAsync(connectionId, "prelogin");
            _logger.LogInformation("✔ Unido al grupo prelogin");
            _logger.LogInformation("────────────────────────────────────────────");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            string connectionId = Context.ConnectionId;

            foreach (var kvp in _instancias)
            {
                if (kvp.Value == connectionId)
                {
                    _instancias.TryRemove(kvp.Key, out _);
                    _logger.LogWarning("❌ Instancia desconectada: {0}", kvp.Key);
                    break;
                }
            }

            _logger.LogWarning("❌ Cliente desconectado: {0}", connectionId);
            _logger.LogInformation("────────────────────────────────────────────");

            await base.OnDisconnectedAsync(ex);
        }

        public Task JoinGroup(string groupName)
        {
            string connectionId = Context.ConnectionId;
            _logger.LogInformation("➡ JoinGroup solicitado → {0} ({1})", groupName, connectionId);
            return Groups.AddToGroupAsync(connectionId, groupName);
        }

        public Task BindAgent(string idAgente)
        {
            string connectionId = Context.ConnectionId;
            string group = $"agente:{idAgente}";
            _logger.LogInformation("➡ BindAgent → {0} ({1})", group, connectionId);

            return Groups.AddToGroupAsync(connectionId, group);
        }

        // ⭐ MÉTODO NECESARIO PARA AuthController
        public static string? GetLastActiveInstance()
        {
            if (_instancias.IsEmpty)
                return null;

            string last = null;
            foreach (var kvp in _instancias)
                last = kvp.Key;

            return last;
        }
    }
}

