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
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<LandingLightListener> _logging;

        public LandingLightListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<LandingLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, LandingLightData data)
        {
            _logging.LogDebug($"Landing light switch? {data.landingSwitch} 2 {data.landingSwitchLeft} 3 {data.landingSwitchRight} "
                + $"states {data.landingState} {data.landingStateLeft} {data.landingStateRight}");
            hub.Clients.All.SetFromSim("lightsLanding", data.landingSwitch == 1);
        }
    }

//SU5 has hosed all this :-( see https://forums.flightsimulator.com/t/axis-and-ohs-help-and-questions/196415/1089

    [Component]
    public class SetLandingLightsEvent : IEvent
    {
        public string SimEvent() => "LANDING_LIGHTS_SET";
    }

    [Component]
    public class LandingLightsToggleEvent : IEvent
    {
        public string SimEvent() => "LANDING_LIGHTS_TOGGLE";
    }

    [Component]
    public class LandingLightsSetter : ISettable<bool>
    {
        private readonly LandingLightsToggleEvent toggleEvent;
        private readonly SetLandingLightsEvent setEvent;

        public LandingLightsSetter(LandingLightsToggleEvent toggleEvent, SetLandingLightsEvent setEvent)
        {
            this.toggleEvent = toggleEvent;
            this.setEvent = setEvent;
        }

        public string GetId() => "lightsLanding";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            simConnect.SendEvent(value ? setEvent : toggleEvent, value ? 3u : 3u);
        }
    }
}
