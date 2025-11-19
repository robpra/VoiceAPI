using Microsoft.AspNetCore.SignalR;

namespace VoiceAPI.Hubs
{
    public class EventsHub : Hub
    {
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

        // Cliente se une a un grupo
        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Cliente sale de un grupo
        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}

