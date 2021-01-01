using System;
using System.ComponentModel;
using System.Threading;
using Controlzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;

// Based on http://www.prepar3d.com/forum/viewtopic.php?p=44893&sid=3b0bd3aae23dc7b9cb0de012bab9daec#p44893
namespace SimConnectzmo
{
    public class Adapter
    {
        private readonly IHubContext<LightHub, ILightHub> hub;

        BackgroundWorker? bw;

        public Adapter(IHubContext<LightHub, ILightHub> hub)
        {
            this.hub = hub;
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
        private static readonly IntPtr hWnd = IntPtr.Zero;

        private void Donkey(object? sender, DoWorkEventArgs args)
        {
            try
            {
                AutoResetEvent MessageSignal = new AutoResetEvent(false);
                using var sc = new SimConnect("Controlzmo", hWnd, WM_USER_SIMCONNECT, MessageSignal, 0u);
                while (!bw!.CancellationPending)
                {
                    if (MessageSignal.WaitOne(1000))
                    {
                        sc.ReceiveMessage();
                        hub.Clients.All.ShowMessage("Got somet' from SimConnect");
                    }
                    else
                        hub.Clients.All.ShowMessage("Got nowt from SimConnect");
                }
            }
            catch (Exception e)
            {
                hub.Clients.All.ShowMessage($"Exception from SimConnect: {e.Message}");
                bw = null;
            }
        }
    }
}
