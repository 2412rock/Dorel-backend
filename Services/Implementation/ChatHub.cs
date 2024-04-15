using Microsoft.AspNetCore.SignalR;

namespace DorelAppBackend.Services.Implementation
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string fromUser, string toUser, string message)
        {
            await Clients.All.SendAsync($"{toUser}",fromUser, message);
        }


        /* public async Task SendMessage(string recipientId, string senderId, string message)
         {
             await Clients.All.SendAsync("2412rock", senderId, message);
         }
     }*/
    }
}
