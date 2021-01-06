using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Controlzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;

// Based on http://www.prepar3d.com/forum/viewtopic.php?p=44893&sid=3b0bd3aae23dc7b9cb0de012bab9daec#p44893
namespace SimConnectzmo
{
    [Component]
    public class Adapter
    {
private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly SimConnectHolder holder;
        private readonly IServiceProvider serviceProvider;

        BackgroundWorker? bw;

        public Adapter(IServiceProvider serviceProvider)
        {
this.hub = serviceProvider.GetRequiredService<IHubContext<LightHub, ILightHub>>();
            this.holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            this.serviceProvider = serviceProvider;
        }

        public void EnsureConnectionIfPossible()
        {
            if (bw == null)
            {
                bw = new BackgroundWorker() { WorkerSupportsCancellation = true };
                bw.DoWork += Donkey;
                bw.RunWorkerAsync();
            }
        }

        private const uint WM_USER_SIMCONNECT = 0x0402;

        private void Donkey(object? sender, DoWorkEventArgs args)
        {
            try
            {
                AutoResetEvent MessageSignal = new AutoResetEvent(false);
                using var esc = new ExtendedSimConnect("Controlzmo", WM_USER_SIMCONNECT, MessageSignal)
                    .AssignIds(serviceProvider);
                holder.SimConnect = esc;
                while (!bw!.CancellationPending)
                {
                    if (MessageSignal.WaitOne(5_000))
                    {
                        esc.ReceiveMessage();
hub.Clients.All.ShowMessage("Got somet' from SimConnect");
                    }
                    else
hub.Clients.All.ShowMessage("Got nowt from SimConnect");
                }
            }
            catch (Exception e)
            {
                holder.SimConnect = null;
hub.Clients.All.ShowMessage($"Exception from SimConnect: {e.Message}");
                bw = null;
            }
        }
    }
}
