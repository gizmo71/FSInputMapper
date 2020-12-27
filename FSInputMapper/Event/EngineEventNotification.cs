using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{


    [Singleton]
    public class Throttle1SetEvent : IEvent { public string SimEvent() { return "THROTTLE1_SET"; } }

    [Singleton]
    public class Throttle2SetEvent : IEvent { public string SimEvent() { return "THROTTLE2_SET"; } }

    public abstract class ThrottleSetEventNotification : IEventNotification
    {
        protected const uint MAGNITUDE_RANGE = 0x4000u;
        private readonly DebugConsole dc;
        private readonly IEvent trigger;

        protected ThrottleSetEventNotification(DebugConsole dc, IEvent trigger) {
            this.dc = dc;
            this.trigger = trigger;
        }

        public IEvent GetEvent() { return trigger; }
        public GROUP GetGroup() { return GROUP.ENGINE; }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            double axis = (double)(data.dwData + MAGNITUDE_RANGE) - MAGNITUDE_RANGE;
            dc.Text = $"Event {(EVENT)data.uEventID} raw {data.dwData} axis {axis}";
            axis = map(axis) + MAGNITUDE_RANGE;
            simConnect.SendEvent((EVENT)data.uEventID, (uint)axis - MAGNITUDE_RANGE);
        }

        protected abstract double map(double axis);
    }

    [Singleton]
    public class Throttle1SetEventNotification : ThrottleSetEventNotification
    {
        public Throttle1SetEventNotification(DebugConsole dc, Throttle1SetEvent e) : base(dc, e) { }

        protected override double map(double axis)
        {
            return axis;
        }
    }

    [Singleton]
    public class Throttle2SetEventNotification : ThrottleSetEventNotification
    {
        public Throttle2SetEventNotification(DebugConsole dc, Throttle2SetEvent e) : base(dc, e) { }

        protected override double map(double axis)
        {
            if (axis < 0) axis = -MAGNITUDE_RANGE;
            return axis;
        }
    }

}