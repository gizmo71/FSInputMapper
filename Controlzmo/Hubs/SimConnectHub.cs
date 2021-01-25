//#define FROTTLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Hubs
{
    // server to client messages
    public partial interface IControlzmoHub
    {
        public Task ShowMessage(string message);
        public Task SetFromSim(string name, bool? value);
    }

    public partial class ControlzmoHub : Hub<IControlzmoHub>
    {
        private readonly SimConnectHolder holder;
        private readonly ILogger<ControlzmoHub> _logger;
        private readonly IDictionary<string, ISettable> settables;
#if FROTTLE
        private readonly IEvent frottle;
#endif

        public ControlzmoHub(IServiceProvider serviceProvider)
        {
            holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            _logger = serviceProvider.GetRequiredService<ILogger<ControlzmoHub>>();
#if FROTTLE
            frottle = serviceProvider.GetRequiredService<Systems.ThrustLevers.Throttle1SetEvent>();
            _ = serviceProvider.GetRequiredService<Systems.ThrustLevers.Throttle1SetEventNotification>();
            _ = serviceProvider.GetRequiredService<Systems.ThrustLevers.Throttle2SetEventNotification>();
#endif

            settables = serviceProvider
                .GetServices<ISettable>()
                .ToDictionary(settable => settable.GetId(), settable => settable);
        }

#if FROTTLE
        public async Task SetFrottle(uint value)
        {
            _logger.LogDebug($"Calling frottle with {value}");
            holder.SimConnect?.SendEvent(frottle, value - 0x4000u);
            await Task.CompletedTask;
        }
#endif

        public async Task SendAll()
        {
            _logger.LogInformation("Triggering initial data requests");
            holder.SimConnect?.TriggerInitialRequests();
            await Task.CompletedTask;
        }

        public async Task SetInSim(string item, bool value)
        {
            ExtendedSimConnect? simConnect = holder.SimConnect;
            if (simConnect == null)
            {
                _logger.LogError($"Can't set {item} to {value}; no SimConnection");
                return;
            }

            _logger.LogDebug($"Setting {item} to {value} at {System.DateTime.Now}");
            settables[item].SetInSim(simConnect!, value);

            await Task.CompletedTask;
        }
    }
}
