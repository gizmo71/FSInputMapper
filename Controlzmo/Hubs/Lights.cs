using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SimConnectzmo;

namespace Controlzmo.Hubs
{
    // server to client messages
    public interface ILightHub
    {
        public Task ShowMessage(string message);
    }

    // client to server messages
    public class LightHub : Hub<ILightHub>
    {
        public LightHub(EnsureConnectionTimer timer)
        {
            timer.Start();
        }

        public async Task ChangedSomet(string message)
        {
            await Clients.All.ShowMessage(message + " " + System.DateTime.Now);
        }

        public async Task SendAll()
        {
            await Clients.All.ShowMessage("Would send all " + System.DateTime.Now + " " + Context.Items);
        }
    }
}
