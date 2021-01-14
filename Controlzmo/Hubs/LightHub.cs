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
        private readonly SetStrobeLightsEvent setStrobeLightsEvent;
        private readonly ToggleBeaconLightsEvent toggleBeaconLightsEvent;
        private readonly ToggleWingIceLightsEvent toggleWingIceLightsEvent;
        private readonly SetNavLightsEvent setNavLightsEvent;
        private readonly SetLogoLightsEvent setLogoLightsEvent;
        private readonly SetTaxiTurnoffLightsEvent setTaxiTurnoffLightsEvent;
        private readonly SetLandingLightsEvent setLandingLightsEvent;
        private readonly ILogger<LightHub> _logger;

        public LightHub(SimConnectHolder holder, SetNavLightsEvent setNavLightsEvent, SetLogoLightsEvent setLogoLightsEvent, SetTaxiTurnoffLightsEvent setTaxiTurnoffLightsEvent, SetLandingLightsEvent setLandingLightsEvent, SetStrobeLightsEvent setStrobeLightsEvent, ToggleBeaconLightsEvent toggleBeaconLightsEvent, ToggleWingIceLightsEvent toggleWingIceLightsEvent, ILogger<LightHub> _logger)
        {
            this.holder = holder;
            this._logger = _logger;
            this.setStrobeLightsEvent = setStrobeLightsEvent;
            this.toggleBeaconLightsEvent = toggleBeaconLightsEvent;
            this.toggleWingIceLightsEvent = toggleWingIceLightsEvent;
            this.setNavLightsEvent = setNavLightsEvent;
            this.setLogoLightsEvent = setLogoLightsEvent;
            this.setLandingLightsEvent = setLandingLightsEvent;
            this.setTaxiTurnoffLightsEvent = setTaxiTurnoffLightsEvent;
        }

        public async Task SetInSim(string item, bool value)
        {
            _logger.LogDebug($"Changing {item} to {value} at {System.DateTime.Now}");
            switch (item)
            {
                case "Strobe":
                    holder.SimConnect?.SendEvent(setStrobeLightsEvent, value ? 1u : 0u);
                    break;
                case "Beacon":
                    holder.SimConnect?.SendEvent(toggleBeaconLightsEvent);
                    break;
                case "WingIce":
                    holder.SimConnect?.SendEvent(toggleWingIceLightsEvent);
                    break;
                case "NavLogo":
                    holder.SimConnect?.SendEvent(setNavLightsEvent, value ? 1u : 0u);
                    holder.SimConnect?.SendEvent(setLogoLightsEvent, value ? 1u : 0u);
                    break;
                case "TaxiTurnoff":
                    holder.SimConnect?.SendEvent(setTaxiTurnoffLightsEvent, value ? 1u : 0u);
                    break;
                case "Landing":
                    holder.SimConnect?.SendEvent(setLandingLightsEvent, value ? 1u : 0u);
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
