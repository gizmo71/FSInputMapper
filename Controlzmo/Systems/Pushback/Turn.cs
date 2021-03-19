using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackTurnData
    {
        [SimVar("PUSHBACK STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int pushbackState;

        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.01f)]
        public int trueHeading;
    };

    [Component]
    public class PushbackTurnListener : DataListener<PushbackTurnData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PushbackTurnListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public override void Process(ExtendedSimConnect simConnect, PushbackTurnData data)
        {
            System.Console.Error.WriteLine($"Pushback: state={data.pushbackState}, heading={data.trueHeading}");
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

        public UInt32 ToData(Double degrees) => ((UInt32)(degrees * 11930465)) & 0xffffffff;
    }

    [Component]
    public class TugHeading : ISettable<string?>
    {
        private readonly TugHeadingEvent setEvent;

        public TugHeading(TugHeadingEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public string GetId() => "tugHeading";

        public void SetInSim(ExtendedSimConnect simConnect, string? degreesAsString)
        {
            //TODO: request PushbackTurnData and only then send the event if state 0, based on relative heading.
            // Alternatively, track the state, since we can't signal desired heading to the data listener.
            var degrees = Double.Parse(degreesAsString!) % 360.0;
            simConnect.SendEvent(setEvent, setEvent.ToData(degrees));
        }
    }
}
