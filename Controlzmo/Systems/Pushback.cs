using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{
    /*A32NX EFB notes
    // The final number in use*SimVar is maximum stale time in milliseconds.
    const [tugHeading, setTugHeading] = useSplitSimVar('PLANE HEADING DEGREES TRUE', 'degrees', 'K:KEY_TUG_HEADING', 'UINT32', 1000);
    const [pushBack, setPushBack] = useSplitSimVar('PUSHBACK STATE', 'enum', 'K:TOGGLE_PUSHBACK', 'bool', 1000);
    const [pushBackWait, setPushBackWait] = useSimVar('Pushback Wait', 'bool', 100);
    const [pushBackAttached] = useSimVar('Pushback Attached', 'bool', 1000);
    const [tugDirection, setTugDirection] = useState(0);
    const [tugActive, setTugActive] = useState(false);

    */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackData
    {
        [SimVar("PUSHBACK STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // 3 = not attached? 0 = straight, P3D says 1=left and 2=right but doesn't seem to match
        // In theory this is settable.
        public int pushbackState;

        [SimVar("PUSHBACK ANGLE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        // Not settable; default values seem to be 20 (tail to the right, heading decreasing) or -20 (tail to the left, heading increasing)
        public int pushbackAngle;

        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        public int trueHeading;

        [SimVar("PUSHBACK ATTACHED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // Doesn't seem to do anything; may not be a real variable...
        public int pushbackAttached;

        [SimVar("PUSHBACK WAIT", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // In theory this should say true if waiting for the tug after calling it, but doesn't seem to work.
        // The EFB code suggests that after calling the tug, this variable is no longer useful.
        public int pushbackWait;
    };

    [Component]
    public class TugHeadingEvent : IEvent
    {
        // **Triggers tug** and sets the desired heading.
        // The units are a 32 bit integer (0 to 4294967295) which represent 0 to 360 degrees.
        // To set a 45 degree angle, for example, set the value to 4294967295 / 8.
        // Or ((90or270+planeTrueHeading) * 11930465) & 0xffffffff
        public string SimEvent() => "TUG_HEADING";
    }

    [Component]
    public class TugSpeedEvent : IEvent
    {
        // **Triggers tug**, and sets desired speed, in feet per second.
        // The speed can be both positive (forward movement) and negative (backward movement).
        public string SimEvent() => "TUG_SPEED";
    }

    [Component]
    public class TugToggleEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_PUSHBACK";
    }

    [Component]
    public class TugDisableEvent : IEvent
    {
        public string SimEvent() => "TUG_DISABLE";
    }

    [Component]
    public class PushbackListener : DataListener<PushbackData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PushbackListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, PushbackData data)
        {
            System.Console.Error.WriteLine($"Pushback: state/attached={data.pushbackState}/{data.pushbackAttached} wait={data.pushbackWait}, angle={data.pushbackAngle}, heading={data.trueHeading}");
            //hub.Clients.All.SetFromSim("lightsBeacon", data.beaconSwitch == 1);
        }
    }
}
