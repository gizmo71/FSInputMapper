using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TrueHeadingData
    {
        [SimVar("PLANE HEADING DEGREES TRUE", "Degrees", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int trueHeading;
    };

    [Component]
    public class TrueHeadingListener : DataListener<TrueHeadingData>, IRequestDataOnOpen
    {
        //TODO: only needed whilst pushback active?
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, TrueHeadingData data)
        {
            System.Console.Error.WriteLine($"True heading={data.trueHeading}");
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

    //TODO: consider being able to set this using the rudder input
    [Component]
    public class TugHeading : ISettable<string?>
    {
        private readonly TugHeadingEvent setEvent;
        private readonly PushbackToggleListener stateListener;

        public TugHeading(TugHeadingEvent setEvent, PushbackToggleListener stateListener)
        {
            this.setEvent = setEvent;
            this.stateListener = stateListener;
        }

        public string GetId() => "tugHeading";

        public void SetInSim(ExtendedSimConnect simConnect, string? degreesAsString)
        {
            if (!stateListener.IsPushbackActive)
            {
                Console.Error.WriteLine("Pushback not active");
                return;
            }
            //TODO: request PushbackTurnData and only then send the event if state 0, based on relative heading.
            // Alternatively, track the state, since we can't signal desired heading to the data listener.
            var degrees = Double.Parse(degreesAsString!) % 360.0;
            simConnect.SendEvent(setEvent, setEvent.ToData(degrees));
        }
    }
}
