using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public Task SetFromSim(string name, object? value);
        public Task Speak(string text);
        public Task SetMcduType(string type);
    }

    public partial class ControlzmoHub : Hub<IControlzmoHub>
    {
        private readonly SimConnectHolder holder;
        private readonly ILogger<ControlzmoHub> _logger;
        private readonly IDictionary<string, ISettable> settables;

        public ControlzmoHub(IServiceProvider serviceProvider)
        {
            holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            _logger = serviceProvider.GetRequiredService<ILogger<ControlzmoHub>>();

            settables = serviceProvider
                .GetServices<ISettable>()
                .ToDictionary(settable => settable.GetId(), settable => settable);
        }

        public async Task SendAll()
        {
            _logger.LogInformation("Triggering initial data requests");
            holder.SimConnect?.TriggerInitialRequests();
            await Task.CompletedTask;
        }

        public async Task SetInSim(string item, JsonElement value)
        {
            ExtendedSimConnect? simConnect = holder.SimConnect;
            if (simConnect == null)
            {
                _logger.LogError($"Can't set {item} to {value}; no SimConnection");
//                return;
            }

            ISettable rawSettable = settables[item];
            var typedValue = JsonSerializer.Deserialize(value.GetRawText(), rawSettable.GetValueType());
            _logger.LogDebug($"Setting {item} to {typedValue}");
            rawSettable.SetInSim(simConnect!, typedValue);

            await Task.CompletedTask;
        }
    }
}
