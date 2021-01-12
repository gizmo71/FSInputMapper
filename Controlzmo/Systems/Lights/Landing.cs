using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingLightData
    {
        // No way to detect "Off" position between On and Retract.
        [SimVar("LIGHT LANDING", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitch;
        [SimVar("LIGHT LANDING:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchLeft;
        [SimVar("LIGHT LANDING:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchRight;
        [SimVar("LIGHT LANDING ON", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingState;
        [SimVar("LIGHT LANDING ON:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingStateLeft;
        [SimVar("LIGHT LANDING ON:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingStateRight;
    };

    [Component]
    public class LandingLightListener : DataListener<LandingLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly ILogger<LandingLightListener> _logging;

        public LandingLightListener(IHubContext<LightHub, ILightHub> hub, ILogger<LandingLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, LandingLightData data)
        {
            _logging.LogDebug($"Landing light on? {data.landingState == 1}");
            hub.Clients.All.SetFromSim("Landing", data.landingState == 1);
        }
    }

    [Component]
    public class SetLandingLightsEvent : IEvent
    {
        public string SimEvent() { return "LANDING_LIGHTS_SET"; }
    }
}
