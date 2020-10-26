using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{

    public abstract class SpoilerEventNotification : IEventNotification
    {

        protected readonly DataListener<SpoilerData> listener;

        public GROUP GetGroup()
        {
            return GROUP.SPOILERS;
        }

        protected SpoilerEventNotification(DataListener<SpoilerData> listener)
        {
            this.listener = listener;
        }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_PERIOD.ONCE);
        }

    }

    [Singleton]
    public class LessSpoilerEventNotification : SpoilerEventNotification
    {
        public LessSpoilerEventNotification(LessSpoilerListener listener) : base(listener) { }
    }

    [Singleton]
    public class MoreSpoilerEventNotification : SpoilerEventNotification
    {
        public MoreSpoilerEventNotification(MoreSpoilerListener listener) : base(listener) { }
    }

}