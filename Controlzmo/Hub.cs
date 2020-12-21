using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Controlzmo
{
    public class TestHub : Hub
    {
        // Nothing magic about user, it and message are just arguments
        public async Task TestMessage(string message)
        {
            await Clients.All.SendAsync("TestMessage", message);
        }
    }
}
