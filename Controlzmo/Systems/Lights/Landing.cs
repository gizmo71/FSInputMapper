using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
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
        [SimVar("LIGHT LANDING:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchLeft;
        [SimVar("LIGHT LANDING:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchRight;
        [SimVar("LIGHT LANDING ON:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingStateLeft;
        [SimVar("LIGHT LANDING ON:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingStateRight;
    };

    [Component]
    public class LandingLightListener : DataListener<LandingLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logging;

        public LandingLightListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<LandingLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, LandingLightData data)
        {
            _logging.LogDebug($"Landing light switch? L {data.landingSwitchLeft} R {data.landingSwitchRight} states L {data.landingStateLeft} R {data.landingStateRight}");
            hub.Clients.All.SetFromSim("lightsLanding", data.landingSwitchLeft == 1 || data.landingSwitchRight == 1);
        }
    }

    [Component]
    public class LandingLightsSetter : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public LandingLightsSetter(JetBridgeSender sender)
        {
            this.sender = sender;
        }

        public string GetId() => "lightsLanding";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            var inv = value ? 0 : 1;
            sender.Execute(simConnect, $"{inv} (>L:LANDING_2_Retracted,·Bool) {inv} (>L:LANDING_3_Retracted,·Bool)"
                + " 2 (>K:LANDING_LIGHTS_TOGGLE) 3 (>K:LANDING_LIGHTS_TOGGLE)");
        }
    }
}
