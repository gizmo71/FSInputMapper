using Controlzmo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _log;
        private readonly AutoResetEvent MessageSignal = new AutoResetEvent(false);

        public Adapter(IServiceProvider sp) : base(sp)
        {
            this.serviceProvider = sp;
            _log = serviceProvider.GetRequiredService<ILogger<Adapter>>();
            holder = serviceProvider.GetRequiredService<SimConnectHolder>();
        }

        private const uint WM_USER_SIMCONNECT = 0x0402;

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
_log.LogInformation("Adapter starting <<<--->>>");
            holder.SimConnect = new ExtendedSimConnect("Controlzmo", WM_USER_SIMCONNECT, MessageSignal)
                .AssignIds(serviceProvider);
_log.LogInformation("Adapter started <<<--->>>");
        }

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            if (MessageSignal!.WaitOne(5_000))
            {
                holder.SimConnect!.ReceiveMessage();
            }
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            holder.SimConnect!.Dispose();
            holder.SimConnect = null;
        }
    }
}
