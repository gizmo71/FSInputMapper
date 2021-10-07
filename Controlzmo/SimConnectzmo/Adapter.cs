using Controlzmo;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Threading;

// Based on http://www.prepar3d.com/forum/viewtopic.php?p=44893&sid=3b0bd3aae23dc7b9cb0de012bab9daec#p44893
namespace SimConnectzmo
{
    [Component]
    public class Adapter : KeepAliveWorker, CreateOnStartup
    {
        private readonly SimConnectHolder holder;
        private readonly IServiceProvider serviceProvider;
        private readonly AutoResetEvent MessageSignal = new AutoResetEvent(false);

        public Adapter(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            this.serviceProvider = serviceProvider;
        }

        private const uint WM_USER_SIMCONNECT = 0x0402;

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            holder.SimConnect = new ExtendedSimConnect("Controlzmo", WM_USER_SIMCONNECT, MessageSignal)
                .AssignIds(serviceProvider);
        }

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            if (MessageSignal!.WaitOne(5_000))
            {
                holder.SimConnect!.ReceiveMessage();
//_logger.LogInformation("Got somet' from SimConnect");
            }
//else _logger.LogDebug("Got nowt from SimConnect");
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            holder.SimConnect!.Dispose();
            holder.SimConnect = null;
        }
    }
}
