﻿using System.Collections.Generic;
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
//private readonly DebugConsole dc;
        private readonly IEvent trigger;
        private readonly IReadOnlyDictionary<uint, uint> raw2fs;

        protected ThrottleSetEventNotification(IEvent trigger, SortedDictionary<uint, uint> raw2fs, DebugConsole dc) {
//this.dc = dc;
            this.trigger = trigger;
            this.raw2fs = raw2fs;
        }

        public IEvent GetEvent() { return trigger; }
        public GROUP GetGroup() { return GROUP.ENGINE; }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            var shifted = data.dwData + MAGNITUDE_RANGE;
            var mapped = MapAxis(shifted);
//dc.Text = $"Event {(EVENT)data.uEventID} shifted {shifted}->{mapped} (raw {data.dwData})";
            simConnect.SendEvent((EVENT)data.uEventID, (uint)mapped - MAGNITUDE_RANGE);
        }

        private readonly KeyValuePair<uint, uint> DUMMY = new(MAGNITUDE_RANGE, MAGNITUDE_RANGE);

        private uint MapAxis(uint raw)
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
            [0] = 13220, // Max reverse
            [7999] = 16100, // Idle reverse
            [8000] = 16384, // Start of idle
            [9200] = 16384, // End of idle
            [9201] = 16550,
            [16600] = 29500,
            [16601] = 31000, // Start of climb
            [17000] = 31000, // End of climb
            [17001] = 31151,
            [24099] = 31780,
            [24100] = 32000, // Start of Flex/MCT
            [25500] = 32000, // End of Flex/MCT
            [25501] = 32210,
            [32766] = 32758,
            [32767] = 32768, // Start of TO/GA
            [32768] = 32768, // End of TO/GA
        };
        public Throttle1SetEventNotification(DebugConsole dc, Throttle1SetEvent e) : base(e, map, dc) { }
    }

    [Singleton]
    public class Throttle2SetEventNotification : ThrottleSetEventNotification
    {
        private static SortedDictionary<uint, uint> map = new SortedDictionary<uint, uint>
        {
            [0] = 13220, // Max reverse
            [7999] = 16100, // Idle reverse
            [8000] = 16384, // Start of idle
            [9200] = 16384, // End of idle
            [9201] = 16550,
            [16600] = 29500,
            [16601] = 31000, // Start of climb
            [17000] = 31000, // End of climb
            [17001] = 31151,
            [23949] = 31780,
            [23950] = 32000, // Start of Flex/MCT
            [25500] = 32000, // End of Flex/MCT
            [25501] = 32210,
            [32766] = 32758,
            [32767] = 32768, // Start of TO/GA
            [32768] = 32768, // End of TO/GA
        };
        public Throttle2SetEventNotification(DebugConsole dc, Throttle2SetEvent e) : base(e, map, dc) { }
    }

}