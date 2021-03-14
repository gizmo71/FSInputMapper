using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackData
    {
        [SimVar("PUSHBACK STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // 3 = not attached? 0 = straight, P3D says 1=left and 2=right but doesn't seem to match
        public int pushbackState; // Set using K:TOGGLE_PUSHBACK and maybe K:TUG_DISABLE - see also K:TUG_SPEED
        [SimVar("PUSHBACK ATTACHED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int pushbackAttached;
        [SimVar("PUSHBACK WAIT", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int pushbackWait;
        [SimVar("PUSHBACK ANGLE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        public int pushbackAngle; // Default seems to be 20 (tail to the right, heading decreasing) or -20 (tail to the left, heading increasing)
        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        public int trueHeading; // Set using K:KEY_TUG_HEADING
    };

    [Component]
    public class PushbackListener : DataListener<PushbackData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<PushbackData> _logging;

        public PushbackListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<PushbackData> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, PushbackData data)
        {
            _logging.LogDebug($"Pushback: state/attached/wait={data.pushbackState}/{data.pushbackAttached}/{data.pushbackWait}, angle={data.pushbackAngle}, heading={data.trueHeading}");
            //hub.Clients.All.SetFromSim("lightsBeacon", data.beaconSwitch == 1);
        }
    }
}
