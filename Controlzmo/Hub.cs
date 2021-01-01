using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using SimConnectzmo;

namespace Controlzmo
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

    [Component]
    public class EnsureConnectionTimer : System.Timers.Timer
    {
        public EnsureConnectionTimer(IHubContext<LightHub, ILightHub> hub, Adapter adapter) : base(1000)
        {
            this.Elapsed += (object sender, ElapsedEventArgs args) => adapter.EnsureConnectionIfPossible();
        }
    }
}
