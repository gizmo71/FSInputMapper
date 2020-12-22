using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{


    [Singleton]
    public class Throttle1SetEvent : IEvent { public string SimEvent() { return "THROTTLE1_AXIS_SET_EX1"; } }

    [Singleton]
    public class Throttle2SetEvent : IEvent { public string SimEvent() { return "THROTTLE2_AXIS_SET_EX1"; } }

    public abstract class ThrottleSetEventNotification : IEventNotification
    {

        private readonly IEvent trigger;

        protected ThrottleSetEventNotification(IEvent trigger) { this.trigger = trigger; }

        public IEvent GetEvent() { return trigger; }
        public GROUP GetGroup() { return GROUP.ENGINE; }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.SendEvent((EVENT)data.uEventID, data.dwData);
            double axis = ((int)data.dwData + 16384) / 327.68;
        }

    }

    [Singleton]
    public class Throttle1SetEventNotification : ThrottleSetEventNotification
    {
        public Throttle1SetEventNotification(Throttle1SetEvent e) : base(e) { }
    }

    [Singleton]
    public class Throttle2SetEventNotification : ThrottleSetEventNotification
    {
        public Throttle2SetEventNotification(Throttle2SetEvent e) : base(e) { }
    }

}