﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

using UintRange = System.Tuple<uint, uint>;

namespace Controlzmo.Systems.ThrustLevers
{
    [Component]
    public class Throttle1SetEvent : IEvent
    {
        public string SimEvent() => "THROTTLE1_SET";
    }

    [Component]
    public class Throttle2SetEvent : IEvent
    {
        public string SimEvent() => "THROTTLE2_SET";
    }

    public abstract class ThrottleSetEventNotification : IEventNotification
    {
        protected const uint MAGNITUDE_RANGE = 0x4000u;
        private readonly IEvent trigger;
        private readonly IReadOnlyDictionary<uint, uint> raw2fs;

        protected ThrottleSetEventNotification(IEvent trigger, SortedDictionary<uint, uint> raw2fs)
        {
            this.trigger = trigger;
            this.raw2fs = raw2fs;
        }

        protected ThrottleSetEventNotification(IEvent trigger,
            SortedDictionary<UintRange, UintRange> tuples,
            ILogger logger) : this(trigger, explode(tuples))
        {
            foreach (var entry in raw2fs)
                logger.LogDebug($"Map {entry.Key} -> {entry.Value}");
        }

        private static SortedDictionary<uint, uint> explode(SortedDictionary<UintRange, UintRange> tuples)
        {
            SortedDictionary<uint, uint> raw2fs = new();
            foreach (UintRange rawTuple in tuples.Keys)
            {
                UintRange mappedTuple = tuples[rawTuple];
                if (rawTuple.Item1 == rawTuple.Item2)
                    throw new ArgumentException($"{rawTuple} => {mappedTuple} must have different raw values");
                if (rawTuple.Item1 > 0)
                {
                    raw2fs[rawTuple.Item1 - 1] = mappedTuple.Item1 - 1;
                }
                raw2fs[rawTuple.Item1] = mappedTuple.Item1;
                raw2fs[rawTuple.Item2] = mappedTuple.Item2;
                if (rawTuple.Item2 < MAGNITUDE_RANGE * 2)
                {
                    raw2fs[rawTuple.Item2 + 1] = mappedTuple.Item2 + 1;
                }

            }
            return raw2fs;
        }

        public IEvent GetEvent() => trigger;

        public void OnRecieve(ExtendedSimConnect? simConnect, SIMCONNECT_RECV_EVENT data)
        {
            var shifted = data.dwData + MAGNITUDE_RANGE;
            var mapped = MapAxis(shifted);
            if (mapped != null)
                simConnect?.SendEvent(trigger, (uint)mapped - MAGNITUDE_RANGE);
        }

        private readonly KeyValuePair<uint, uint> DUMMY = new(MAGNITUDE_RANGE, MAGNITUDE_RANGE);

        protected virtual uint? MapAxis(uint raw)
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
            double inputFraction = (raw - start.Key) / (end.Key - (double)start.Key);
            return start.Value + (uint)(inputFraction * (end.Value - start.Value));
        }

/* ThrottleConfiguration.ini
[Throttle]
Log = true
Enabled = true
ReverseOnAxis = true
ReverseIdle = true
DetentDeadZone = 2.5
DetentReverseIdle = -0.90
DetentReverseFull = -1.00
DetentIdle = -0.42
DetentClimb = 0.06
DetentFlexMct = 0.53
DetentTakeOffGoAround = 1.00 */
        protected static readonly UintRange MAX_REV = new (0, 273);
        protected static readonly UintRange IDLE_REV = new(1366, 5570);
        protected static readonly UintRange IDLE = new(5571, 9723);
        protected static readonly UintRange CL = new(17147, 20575);
        protected static readonly UintRange FLX_MCT = new(21859, 28917);
        protected static readonly UintRange TO_GA = new(31228, 32768);
    }

    [Component]
    public class Throttle1SetEventNotification : ThrottleSetEventNotification
    {
        private static SortedDictionary<uint, uint> mapOld = new ()
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
        private static SortedDictionary<UintRange, UintRange> map = new ()
        {
            [new UintRange(0, 299)] = MAX_REV,
            [new UintRange(4000, 7999)] = IDLE_REV,
            [new UintRange(8000, 9200)] = IDLE,
            [new UintRange(16601, 17000)] = CL,
            [new UintRange(23900, 25700)] = FLX_MCT,
            [new UintRange(32767, 32768)] = TO_GA,
        };

        public Throttle1SetEventNotification(Throttle1SetEvent e, ILogger<Throttle1SetEventNotification> logger) : base(e, map, logger) { }

        //protected override uint? MapAxis(uint raw) => null;
    }

    [Component]
    public class Throttle2SetEventNotification : ThrottleSetEventNotification
    {
        private static SortedDictionary<uint, uint> mapOld = new ()
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
        private static SortedDictionary<UintRange, UintRange> map = new ()
        {
            [new UintRange(0, 299)] = MAX_REV,
            [new UintRange(4000, 7999)] = IDLE_REV,
            [new UintRange(8000, 9200)] = IDLE,
            [new UintRange(16601, 17000)] = CL,
            [new UintRange(23750, 25700)] = FLX_MCT,
            [new UintRange(32767, 32768)] = TO_GA,
        };

    public Throttle2SetEventNotification(Throttle2SetEvent e, ILogger<Throttle2SetEventNotification> logger) : base(e, map, logger) { }

        //protected override uint? MapAxis(uint raw) => null;
    }
}
