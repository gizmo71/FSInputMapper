using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    public interface IOnSimConnection
    {
        public void OnConnection(ExtendedSimConnect simConnect);
    }

    public interface IOnSimStarted
    {
        public void OnStarted(ExtendedSimConnect simConnect);
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
