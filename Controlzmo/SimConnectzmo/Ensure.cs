using System.Timers;
using Controlzmo;
using Microsoft.AspNetCore.SignalR;

namespace SimConnectzmo
{
    [Component]
    public class EnsureConnectionTimer : System.Timers.Timer
    {
        public EnsureConnectionTimer(IHubContext<LightHub, ILightHub> hub, Adapter adapter) : base(5000)
        {
            this.Elapsed += (object sender, ElapsedEventArgs args) => adapter.EnsureConnectionIfPossible();
        }
    }
}
