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
        // No way to detect "Off" position between On and Retract without L:LANDING_[23]_Retracted.
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
    public class LandingLightSystem : DataListener<LandingLightData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logging;
        private readonly JetBridgeSender sender;

        public LandingLightSystem(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<LandingLightSystem> _logging, JetBridgeSender sender)
        {
            this.hub = hub;
            this._logging = _logging;
            this.sender = sender;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, LandingLightData data)
        {
            _logging.LogDebug($"Landing light switch? L {data.landingSwitchLeft} R {data.landingSwitchRight} states L {data.landingStateLeft} R {data.landingStateRight}");
            oldLeft = data.landingSwitchLeft == 1;
            oldRight = data.landingSwitchRight == 1;
            hub.Clients.All.SetFromSim(GetId(), oldLeft || oldRight);
        }

        bool oldLeft;
        bool oldRight;

        public string GetId() => "lightsLanding";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            var retracted = value ? 0 : 1;
            var code = $"{retracted} (>L:LANDING_2_Retracted,·Bool) {retracted} (>L:LANDING_3_Retracted,·Bool)";
            if (oldLeft != value)
                code += " 2 (>K:LANDING_LIGHTS_TOGGLE)";
            if (oldRight != value)
                code += " 3 (>K:LANDING_LIGHTS_TOGGLE)";
            sender.Execute(simConnect, code);
        }
    }
}
