using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{

    [Singleton]
    public class MoreSpoilerEvent : IEvent
    {
        public string SimEvent() { return "SPOILERS_TOGGLE"; }
    }

    [Singleton]
    public class LessSpoilerEvent : IEvent
    {
        public string SimEvent() { return "SPOILERS_ARM_TOGGLE"; }
    }

    public abstract class SpoilerEventNotification : IEventNotification
    {

        private readonly IEvent trigger;
        private readonly DataListener<SpoilerData> listener;

        protected SpoilerEventNotification(DataListener<SpoilerData> listener, IEvent trigger)
        {
            this.listener = listener;
            this.trigger = trigger;
        }

        public IEvent GetEvent() { return trigger; }
        public GROUP GetGroup() { return GROUP.SPOILERS; }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_PERIOD.ONCE);
        }

    }

    [Singleton]
    public class LessSpoilerEventNotification : SpoilerEventNotification
    {
        public LessSpoilerEventNotification(LessSpoilerListener listener, LessSpoilerEvent e) : base(listener, e) { }
    }

    [Singleton]
    public class MoreSpoilerEventNotification : SpoilerEventNotification
    {
        public MoreSpoilerEventNotification(MoreSpoilerListener listener, MoreSpoilerEvent e) : base(listener, e) { }
    }

}