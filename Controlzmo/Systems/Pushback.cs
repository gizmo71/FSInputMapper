using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{
    /*A32NX EFB notes; the final number in use*SimVar is maximum stale time in milliseconds.
    const [tugHeading, setTugHeading] = useSplitSimVar('PLANE HEADING DEGREES TRUE', 'degrees', 'K:KEY_TUG_HEADING', 'UINT32', 1000);
    const [pushBack, setPushBack] = useSplitSimVar('PUSHBACK STATE', 'enum', 'K:TOGGLE_PUSHBACK', 'bool', 1000);
    const [pushBackWait, setPushBackWait] = useSimVar('Pushback Wait', 'bool', 100);
    const [pushBackAttached] = useSimVar('Pushback Attached', 'bool', 1000);
    const [tugDirection, setTugDirection] = useState(0);
    const [tugActive, setTugActive] = useState(false); */

    // See also https://github.com/metindikbas/msfs-pushback-helper-app/

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackStateData
    {
        [SimVar("PUSHBACK STATE", null, SIMCONNECT_DATATYPE.STRING32, 0.5f)]
        public string pushbackState;
    }

    [Component]
    public class PushbackState : DataSender<PushbackStateData>, ISettable<string?>
    {
        public string GetId() => "pushbackState";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            Console.Error.WriteLine($"Got S {value}");
            // Doesn't seem to have any effect.
            Send(simConnect, new PushbackStateData { pushbackState = $"SELECT_{Int32.Parse(value!)}" });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackWaitData
    {
        [SimVar("PUSHBACK WAIT", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // In theory this should say true if waiting for the tug after calling it, but doesn't seem to work.
        // The EFB code suggests that after calling the tug, this variable is no longer useful, which matches observations.
        public int pushbackWait;
    }

    [Component]
    public class PushbackWait : DataSender<PushbackWaitData>, ISettable<string?>
    {
        public string GetId() => "pushbackWait";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            Console.Error.WriteLine($"Got W {value}");
            Send(simConnect, new PushbackWaitData { pushbackWait = Int32.Parse(value!) });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackData
    {
        [SimVar("PUSHBACK STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // 3 = not attached? 0 = straight, P3D says 1=left and 2=right but doesn't seem to match
        // In theory this is settable. Starts out as 3.
        public int pushbackState;

        [SimVar("PUSHBACK WAIT", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // In theory this should say true if waiting for the tug after calling it, but doesn't seem to work.
        // The EFB code suggests that after calling the tug, this variable is no longer useful, which matches observations.
        public int pushbackWait;

        [SimVar("PUSHBACK ANGLE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        // Not settable; default values seem to be 20 (tail to the right, heading decreasing) or -20 (tail to the left, heading increasing)
        public int pushbackAngle;

        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        public int trueHeading;

        [SimVar("GROUND VELOCITY", "Knots", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float groundKnots;

        [SimVar("TOTAL WORLD VELOCITY", "Feet per second", SIMCONNECT_DATATYPE.FLOAT32, 0.2f)]
        public float velocityTotal;

        [SimVar("PUSHBACK ATTACHED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        // State 0 and this as 1 immediately after toggling pushback, even before tug is attached.
        public int pushbackAttached;

        [SimVar("Brake Parking Position", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int parkingBrakePosition;
        /*simClient.AddToDataDefinition(DefinitionsEnum.RotationX, "Rotation Velocity Body X", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
          simClient.AddToDataDefinition(DefinitionsEnum.RotationY, "Rotation Velocity Body Y", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
          simClient.AddToDataDefinition(DefinitionsEnum.RotationZ, "Rotation Velocity Body Z", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);*/
    };

    //[Component]
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
            System.Console.Error.WriteLine($"Pushback: state={data.pushbackState} attached={data.pushbackAttached} wait={data.pushbackWait}, angle={data.pushbackAngle},\n"
                + $"\theading={data.trueHeading}, knots={data.groundKnots}, parkBrake={data.parkingBrakePosition}, velocity={data.velocityTotal}");
            hub.Clients.All.SetFromSim("pushbackState", data.pushbackState);
            hub.Clients.All.SetFromSim("pushbackAngle", data.pushbackAngle);
            hub.Clients.All.SetFromSim("trueHeading", data.trueHeading);
        }
    }

    [Component]
    public class TugHeadingEvent : IEvent
    {
        // **Triggers tug** and sets the desired heading.
        // The units are a 32 bit integer (0 to 4294967295) which represent 0 to 360 degrees.
        // To set a 45 degree angle, for example, set the value to 4294967295 / 8.
        // Or ((90or270+planeTrueHeading) * 11930465) & 0xffffffff
        public string SimEvent() => "KEY_TUG_HEADING";
    }

    [Component]
    public class TugHeading : ISettable<string?>
    {
        private readonly IEvent setEvent;

        public TugHeading(TugHeadingEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public string GetId() => "tugHeading";

        public void SetInSim(ExtendedSimConnect simConnect, string? degreesAsString)
        {
            var degrees = UInt32.Parse(degreesAsString!) % 360;
            simConnect.SendEvent(setEvent, (degrees * 11930465) & 0xffffffff);
        }
    }

    [Component]
    public class TugSpeedEvent : IEvent
    {
        // **Triggers tug**, and sets desired speed, in feet per second.
        // The speed can be both positive (forward movement) and negative (backward movement).
        // Can't see any evidence of this actually doing anything.
        public string SimEvent() => "KEY_TUG_SPEED";
    }

    [Component]
    public class TugSpeed : ISettable<string?>
    {
        private readonly IEvent setEvent;

        public TugSpeed(TugSpeedEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public string GetId() => "tugSpeed";

        public void SetInSim(ExtendedSimConnect simConnect, string? newSpeed)
        {
            var newSpeedFtPerSecond = Int32.Parse(newSpeed!);
            simConnect.SendEvent(setEvent, (uint)newSpeedFtPerSecond);
        }
    }

    [Component]
    public class TugToggleEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_PUSHBACK";
    }

    [Component]
    public class TugToggle : AbstractButton
    {
        public TugToggle(TugToggleEvent toggleEvent) : base(toggleEvent) { }

        public override string GetId() => "tugToggle";
    }

    [Component]
    public class TugDisableEvent : IEvent
    {
        public string SimEvent() => "TUG_DISABLE";
    }

    [Component]
    public class TugDisable : AbstractButton
    {
        public TugDisable(TugDisableEvent disableEvent) : base(disableEvent) { }

        public override string GetId() => "tugDisable";
    }
}
