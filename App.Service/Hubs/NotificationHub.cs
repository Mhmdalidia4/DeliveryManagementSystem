using Microsoft.AspNetCore.SignalR;

namespace App.Service.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            Console.WriteLine($"User connected: {userId}");
            await base.OnConnectedAsync();
        }

        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}