using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TrueHeadingData
    {
        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float trueHeading;
        [SimVar("RUDDER POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float rudderPosition;
        [SimVar("RUDDER PEDAL POSITION", "Position", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float rudderPedalPosition;
        [SimVar("STEER INPUT CONTROL", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float steerPosition;
    };

    [Component]
    public class TrueHeadingListener : DataListener<TrueHeadingData>
    {
        public override void Process(ExtendedSimConnect simConnect, TrueHeadingData data)
        {
            System.Console.Error.WriteLine($"True heading={data.trueHeading}, pedal {data.rudderPosition} pos {data.rudderPedalPosition}");
        }
    }

    [Component]
    public class TugHeadingEvent : IEvent
    {
        // Argument is an unsigned 32 bit integer which represents 0 to 359.999 degrees min to max value.
        public string SimEvent() => "KEY_TUG_HEADING";

        public UInt32 ToData(Double degrees) => ((UInt32)(degrees * 11930465)) & 0xffffffff;
    }

    //TODO: consider being able to set this using the rudder input - won't be able to look away, obviously.
    [Component]
    public class TugHeading : ISettable<string?>
    {
        private readonly TugHeadingEvent setEvent;
        private readonly PushbackState state;

        public TugHeading(TugHeadingEvent setEvent, PushbackState state)
        {
            this.setEvent = setEvent;
            this.state = state;
        }

        public string GetId() => "tugHeading";

        public void SetInSim(ExtendedSimConnect simConnect, string? degreesAsString)
        {
            if (!state.IsPushbackActive)
            {
                Console.Error.WriteLine("Pushback not active");
                return;
            }
            var degrees = Double.Parse(degreesAsString!) % 360.0;
            simConnect.SendEvent(setEvent, setEvent.ToData(degrees));
        }
    }
}
