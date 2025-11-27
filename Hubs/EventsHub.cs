using Microsoft.AspNetCore.SignalR;

namespace VoiceAPI.Hubs
{
    public class EventsHub : Hub
    {
        // ===========================================================
        // Cliente conectado
        // ===========================================================
        public override async Task OnConnectedAsync()
        {
            string connId = Context.ConnectionId;

            Console.WriteLine($"🔌 Cliente conectado: {connId}");

            // Primera etapa: siempre entra a prelogin
            await Groups.AddToGroupAsync(connId, "prelogin");
            Console.WriteLine($"👥 {connId} agregado a prelogin");

            await base.OnConnectedAsync();
        }

        // ===========================================================
        // Cliente desconectado
        // ===========================================================
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            string connId = Context.ConnectionId;
            Console.WriteLine($"❌ Cliente desconectado: {connId}");
            await base.OnDisconnectedAsync(ex);
        }

        // ===========================================================
        // JoinGroup genérico (instancia, prelogin, etc)
        // ===========================================================
        public async Task JoinGroup(string groupName)
        {
            string connId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connId, groupName);
            Console.WriteLine($"➡ {connId} unido al grupo: {groupName}");
        }

        // ===========================================================
        // LeaveGroup genérico
        // ===========================================================
        public async Task LeaveGroup(string groupName)
        {
            string connId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connId, groupName);
            Console.WriteLine($"⬅ {connId} salió del grupo: {groupName}");
        }

        // ===========================================================
        // BINDAGENTE → GRUPO FINAL DEL AGENTE
        //
        // EJEMPLO desde Javascript:
        //    connection.invoke("BindAgent", "2020")
        //
        // Nombre de grupo usado por tu versión vieja: AGENTE_2020
        // ===========================================================
        public async Task BindAgent(string idAgente)
        {
            string connId = Context.ConnectionId;

            string group = $"AGENTE_{idAgente}";

            await Groups.AddToGroupAsync(connId, group);

            Console.WriteLine($"🎧 {connId} asignado a grupo de agente {group}");
        }

        // ===========================================================
        // DEBUG
        // ===========================================================
        public string Ping()
        {
            return $"pong:{Context.ConnectionId}";
        }
    }
}

