using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Hunarmand.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task RegisterUser(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
    }
}
