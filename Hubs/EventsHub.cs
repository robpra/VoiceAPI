using Microsoft.AspNetCore.SignalR;

namespace VoiceAPI.Hubs
{
    public class EventsHub : Hub
    {
        // ============================================================
        //  CONEXIÓN / DESCONEXIÓN
        // ============================================================

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"🔌 Cliente conectado: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? ex)
        {
            Console.WriteLine($"❌ Cliente desconectado: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(ex);
        }

        // ============================================================
        //  SUSCRIPCIÓN A GRUPOS (prelogin, agente:xxxx)
        // ============================================================

        public Task JoinGroup(string groupName)
        {
            Console.WriteLine($"📌 JoinGroup → {groupName} | ConnID: {Context.ConnectionId}");
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            Console.WriteLine($"🚪 LeaveGroup → {groupName} | ConnID: {Context.ConnectionId}");
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // ============================================================
        //  MÉTODOS HDR (solo para debug opcional)
        // ============================================================

        public Task Ping()
        {
            Console.WriteLine($"📡 Ping recibido desde {Context.ConnectionId}");
            return Task.CompletedTask;
        }
    }
}

