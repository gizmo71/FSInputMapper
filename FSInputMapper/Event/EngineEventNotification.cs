using System.Collections.Generic;
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
        private readonly IReadOnlyDictionary<uint, uint> raw2fs;

        protected ThrottleSetEventNotification(IEvent trigger, SortedDictionary<uint, uint> raw2fs, DebugConsole dc) {
this.dc = dc;
            this.trigger = trigger;
            this.raw2fs = raw2fs;
        }

        public IEvent GetEvent() { return trigger; }
        public GROUP GetGroup() { return GROUP.ENGINE; }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            var shifted = data.dwData + MAGNITUDE_RANGE;
            var mapped = map(shifted);
dc.Text = $"Event {(EVENT)data.uEventID} shifted {shifted}->{mapped} (raw {data.dwData})";
            simConnect.SendEvent((EVENT)data.uEventID, (uint)mapped - MAGNITUDE_RANGE);
        }

        private readonly KeyValuePair<uint, uint> DUMMY = new(MAGNITUDE_RANGE, MAGNITUDE_RANGE);

        private uint map(uint raw)
        {
            var start = DUMMY;
            var end = DUMMY;
            foreach (KeyValuePair<uint, uint> pair in raw2fs)
            {
                if (pair.Key <= raw)
                    start = pair;
                else if (pair.Key >= raw)
                {
                    end = pair;
                    break;
                }
            }
            if (start.Value == end.Value) return start.Value;
            double inputFraction = (raw - start.Key) / (end.Key - (double) start.Key);
            return start.Value + (uint)(inputFraction * (end.Value - start.Value));
        }
    }

    [Singleton]
    public class Throttle1SetEventNotification : ThrottleSetEventNotification
    {
        private static SortedDictionary<uint, uint> map = new SortedDictionary<uint, uint>
        {
            [0] = 0, // Max reverse
            [16383] = 0, // Idle reverse - map to full until engine 1 problem fixed
            [16384] = 16384, // Idle
            [22000] = 17000, // Start of climb
            [23999] = 19999, // End of climb
            [24000] = 20000,
            [25999] = 22999,
            [26000] = 23000, // Start of Flex/MCT
            [27999] = 25999, // End of Flex/MCT
            [28000] = 26000,
            [29999] = 28999,
            [30000] = 29000, // Start of TO/GA
            [32767] = 32767, // End of TO/GA
        };
        public Throttle1SetEventNotification(DebugConsole dc, Throttle1SetEvent e) : base(e, map, dc) { }
    }

    [Singleton]
    public class Throttle2SetEventNotification : ThrottleSetEventNotification
    {
        private static SortedDictionary<uint, uint> map = new SortedDictionary<uint, uint>
        {
            [0] = 0, // Max reverse
            [16383] = 0, // Idle reverse - map to full until engine 1 problem fixed
            [16384] = 16384, // Idle
            [17000] = 22000, // Start of climb
            [19999] = 23999, // End of climb
            [20000] = 24000,
            [22999] = 25999,
            [23000] = 26000, // Start of Flex/MCT
            [25999] = 27999, // End of Flex/MCT
            [26000] = 28000,
            [28999] = 29999,
            [29000] = 30000, // Start of TO/GA
            [32767] = 32767, // End of TO/GA
        };
        public Throttle2SetEventNotification(DebugConsole dc, Throttle2SetEvent e) : base(e, map, dc) { }
    }

}