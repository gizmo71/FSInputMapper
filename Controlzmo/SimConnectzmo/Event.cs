using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    public interface IOnSimFrame
    {
        public void OnFrame(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT_FRAME data);
    }

    /*TODO: be warned that this does not trigger when the app starts.
     * My suspicion is that during startup it comes up too early. */
    public interface IOnSimStarted
    {
        public void OnStarted(ExtendedSimConnect simConnect);
    }

    public interface IOnAircraftLoaded
    {
        public void OnAircraftLoaded(ExtendedSimConnect simConnect);
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public readonly string ClientEvent;

        public EventAttribute(string clientEvent)
        {
            ClientEvent = clientEvent;
        }
    }

    public interface IEvent
    {
        public abstract string SimEvent();
    }

    public interface IEventNotification
    {
        public abstract IEvent GetEvent();
        public abstract void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data);
    }
}
