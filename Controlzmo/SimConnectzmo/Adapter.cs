using System;
using System.ComponentModel;
using System.Threading;
using Controlzmo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Based on http://www.prepar3d.com/forum/viewtopic.php?p=44893&sid=3b0bd3aae23dc7b9cb0de012bab9daec#p44893
namespace SimConnectzmo
{
    [Component]
    public class Adapter : KeepAliveWorker, CreateOnStartup
    {
        private readonly ILogger<Adapter> _logger;
        private readonly SimConnectHolder holder;
        private readonly IServiceProvider serviceProvider;

        public Adapter(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._logger = serviceProvider.GetRequiredService<ILogger<Adapter>>();
            this.holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            this.serviceProvider = serviceProvider;
        }

        private const uint WM_USER_SIMCONNECT = 0x0402;

        protected override void Donkey(object? sender, DoWorkEventArgs args)
        {
            try
            {
                AutoResetEvent MessageSignal = new AutoResetEvent(false);
                using var esc = new ExtendedSimConnect("Controlzmo", WM_USER_SIMCONNECT, MessageSignal)
                    .AssignIds(serviceProvider);
                holder.SimConnect = esc;
                while (!IsCancellationPending())
                {
                    if (MessageSignal.WaitOne(5_000))
                    {
                        esc.ReceiveMessage();
//_logger.LogInformation("Got somet' from SimConnect");
                    }
//else _logger.LogDebug("Got nowt from SimConnect");
                }
            }
            catch (Exception e)
            {
                holder.SimConnect = null;
                _logger.LogError(e, "Exception from SimConnect");
            }
        }
    }
}
