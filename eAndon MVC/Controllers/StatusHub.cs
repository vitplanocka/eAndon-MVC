using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace eAndon_MVC.Hubs
{
    public class StatusHub : Hub
    {
        public async Task StatusUpdate(string workcenterID, int statusIndex, string newStatus)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", workcenterID, statusIndex, newStatus);
        }
    }
}