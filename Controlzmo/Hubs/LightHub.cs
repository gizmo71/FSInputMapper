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
        public Task SetLandingLights(bool value);
    }

    // client to server messages
    public class LightHub : Hub<ILightHub>
    {
        private readonly SimConnectHolder holder;
        private readonly SetLandingLightsEvent setLandingLightsEvent;
        private readonly ILogger<LightHub> _logger;

        public LightHub(SimConnectHolder holder, SetLandingLightsEvent setLandingLightsEvent, ILogger<LightHub> _logger)
        {
            this.holder = holder;
            this._logger = _logger;
            this.setLandingLightsEvent = setLandingLightsEvent;
        }

        public async Task ChangedSomet(string item, bool value)
        {
            _logger.LogDebug($"Changing {item} to {value} at {System.DateTime.Now}");
            switch (item)
            {
                case "lightLanding":
                    holder.SimConnect?.SendEvent(setLandingLightsEvent, value ? 1u : 0u);
                    break;
                default:
                    _logger.LogError($"Dunno what {item} is");
                    break;
            }
        }

        //TODO: move out of the light hub to something more 'central'
        public void SendAll()
        {
            _logger.LogInformation("Triggering initial data requests");
            holder.SimConnect?.TriggerInitialRequests();
        }
    }
}
