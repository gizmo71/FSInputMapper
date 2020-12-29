using Microsoft.AspNetCore.SignalR;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
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
        private readonly WibbleTimer wt;
        public LightHub(WibbleTimer wt)
        {
            this.wt = wt;
        }

        public async Task ChangedSomet(string message)
        {
            await Clients.All.ShowMessage(message + " " + System.DateTime.Now);
        }

        public async Task SendAll()
        {
            wt.Start();
            await Clients.All.ShowMessage("Would send all " + System.DateTime.Now + " " + Context.Items);
        }
    }

    public class WibbleTimer : System.Timers.Timer
    {
        private readonly IHubContext<LightHub, ILightHub> Hub;
        private readonly Adapter adapter;

        public WibbleTimer(IHubContext<LightHub, ILightHub> hub, Adapter adapter) : base(1000)
        {
            this.Hub = hub;
            this.Elapsed += DoStuff;
            this.adapter = adapter;
        }

        private void DoStuff(object sender, ElapsedEventArgs args)
        {
            string scResult = "ain't tried";
            try
            {
                scResult = adapter.TestIt();
            }
            catch (Exception e)
            {
                scResult = e.Message;
            }
            _ = Hub.Clients.All.ShowMessage("\"<tick>\" " + System.DateTime.Now + " " + scResult);
        }
    }
}
