using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
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
        public abstract void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data);
    }
}
