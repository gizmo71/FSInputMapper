using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Controlzmo.Systems.Lights;
using SimConnectzmo;

namespace Controlzmo.Hubs
{
    // server to client messages
    public interface ILightHub
    {
        public Task ShowMessage(string message);
        public Task SetFromSim(string name, bool? value);
    }

    // client to server messages
    public class LightHub : Hub<ILightHub>
    {
        private readonly SimConnectHolder holder;
        private readonly SetLandingLightsEvent setLandingLightsEvent;
        private readonly SetStrobeLightsEvent setStrobeLightsEvent;
        private readonly ToggleBeaconLightsEvent toggleBeaconLightsEvent;
        private readonly ILogger<LightHub> _logger;

        public LightHub(SimConnectHolder holder, SetLandingLightsEvent setLandingLightsEvent, SetStrobeLightsEvent setStrobeLightsEvent, ToggleBeaconLightsEvent toggleBeaconLightsEvent, ILogger<LightHub> _logger)
        {
            this.holder = holder;
            this._logger = _logger;
            this.setLandingLightsEvent = setLandingLightsEvent;
            this.setStrobeLightsEvent = setStrobeLightsEvent;
            this.toggleBeaconLightsEvent = toggleBeaconLightsEvent;
        }

        public async Task SetInSim(string item, bool value)
        {
            _logger.LogDebug($"Changing {item} to {value} at {System.DateTime.Now}");
            switch (item)
            {
                case "Landing":
                    holder.SimConnect?.SendEvent(setLandingLightsEvent, value ? 1u : 0u);
                    break;
                case "Strobe":
                    holder.SimConnect?.SendEvent(setStrobeLightsEvent, value ? 1u : 0u);
                    break;
                case "Beacon":
                    //TODO: should we read it and then react if not set correctly?
                    holder.SimConnect?.SendEvent(toggleBeaconLightsEvent);
                    break;
                default:
                    _logger.LogError($"Dunno what lights {item} are");
                    break;
            }
            await Task.CompletedTask;
        }

        //TODO: move out of the light hub to something more 'central'
        public async Task SendAll()
        {
            _logger.LogInformation("Triggering initial data requests");
            holder.SimConnect?.TriggerInitialRequests();
            await Task.CompletedTask;
        }
    }
}
