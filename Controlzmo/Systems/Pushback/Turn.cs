using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TugHeadingData
    {
        [SimVar("PUSHBACK STATE", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public uint pushbackState;
        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float trueHeading;
        [SimVar("RUDDER PEDAL POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT32, 0.02f)]
        public float rudderPedalPosition;
    };

    [Component]
    public class TugHeadingEvent : IEvent
    {
        // Argument is an unsigned 32 bit integer which represents 0 to 359.999 degrees min to max value.
        public string SimEvent() => "KEY_TUG_HEADING";

        public UInt32 ToData(Double degrees) => ((UInt32)(degrees * 11930465)) & 0xffffffff;
    }

    [Component]
    public class TugHeadingListener : DataListener<TugHeadingData>
    {
        private readonly TugHeadingEvent setEvent;

        public TugHeadingListener(TugHeadingEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public override void Process(ExtendedSimConnect simConnect, TugHeadingData data)
        {
            System.Console.Error.WriteLine($"State={data.pushbackState}, True heading={data.trueHeading}, rudder pedal {data.rudderPedalPosition}");
            if (data.pushbackState != 3)
            {
                var degrees = (data.trueHeading + 360.0 - data.rudderPedalPosition * 50.0) % 360.0;
                simConnect.SendEvent(setEvent, setEvent.ToData(degrees));
            }
            else if (data.pushbackState == 0)
            {
                //TODO: do we need to set the tug heading to the real heading one last time? Is it too late here?
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.NEVER);
            }
        }

        internal void OnPushbackStart(ExtendedSimConnect simConnect)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        }
    }
}
