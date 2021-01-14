using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NavLogoLightData
    {
        [SimVar("LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int navSwitch;
        [SimVar("LIGHT NAV ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int navState; // Locked to logo
        [SimVar("LIGHT LOGO", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int logoSwitch;
        [SimVar("LIGHT LOGO ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int logoState; // Locked to nav
    };

    [Component]
    public class NavLogoLightListener : DataListener<NavLogoLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly ILogger<NavLogoLightListener> _logging;

        public NavLogoLightListener(IHubContext<LightHub, ILightHub> hub, ILogger<NavLogoLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, NavLogoLightData data)
        {
            _logging.LogDebug($"Nav light on? {data.navState} Logo light on? {data.logoState}");
            hub.Clients.All.SetFromSim("NavLogo", data.navSwitch + data.logoSwitch != 0);
        }
    }

    [Component]
    public class SetNavLightsEvent : IEvent
    {
        public string SimEvent() { return "NAV_LIGHTS_SET"; }
    }

    [Component]
    public class SetLogoLightsEvent : IEvent
    {
        public string SimEvent() { return "LOGO_LIGHTS_SET"; }
    }
}
