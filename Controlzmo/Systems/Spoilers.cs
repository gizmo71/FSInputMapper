using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Spoilers
{
    /* A32NX rules:
       You cannot arm the spoilers unless the handle is RETRACTED.
       If the new position is anything but that, the arming state is false.
       Ground spoilers can be active during RTO even if not armed, in which case handle position becomes FULL.
       Similar but more complex rules during landing, and position can be PARTIAL without showing active.
       The "SPOILERS HANDLE POSITION" is the sim position, NOT the visible position, A32NX_SPOILERS_HANDLE_POSITION (0-1). */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [SimVar("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 armed;
        [SimVar("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 position;
        [SimVar("L:A32NX_SPOILERS_ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxArmed;
        [SimVar("L:A32NX_SPOILERS_GROUND_SPOILERS_ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxGroundSpoilersActive;
        [SimVar("L:A32NX_SPOILERS_HANDLE_POSITION", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.025f)]
        public float a32nxPosition;
        [SimVar("L:A32NX_IS_READY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxReady;
    };

    [Component] public class SpoilerArmOnEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_ON"; }
    [Component] public class SpoilerArmOffEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_OFF"; }
    [Component] public class SetSpoilerHandleEvent : IEvent { public string SimEvent() => "SPOILERS_SET"; }

    public abstract class AbstractSpoilerDataListener : DataListener<SpoilerData>
    {
        protected readonly SetSpoilerHandleEvent setEvent;
        protected readonly ILogger _logger;

        protected AbstractSpoilerDataListener(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);
            setEvent = sp.GetRequiredService<SetSpoilerHandleEvent>();
        }

        public override void Process(ExtendedSimConnect simConnect, SpoilerData data)
        {
            _logger.LogDebug($"Wants spoiler; raw data pos {data.position} armed {data.armed} A32NX ready? {data.a32nxReady} A32NX armed {data.a32nxArmed} active {data.a32nxGroundSpoilersActive} handle {data.a32nxPosition}");
            if (data.a32nxReady == 1) {
                if (data.a32nxArmed == 1 || data.a32nxGroundSpoilersActive == 1) data.armed = 1;
                data.position = (int)(100 * data.a32nxPosition);
            }
            _logger.LogDebug($"... processed data pos {data.position} armed {data.armed}");

            ProcessSpoilerDemand(simConnect, data);
        }

        protected abstract void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data);
    }

    [Component]
    public class MoreSpoiler : AbstractSpoilerDataListener
{
        private readonly SpoilerArmOffEvent armOffEvent;

        public MoreSpoiler(IServiceProvider sp) : base(sp)
        {
            armOffEvent = sp.GetRequiredService<SpoilerArmOffEvent>();
        }

        protected override void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data)
        {
            int? newPosition = null;
            IEvent? toSend = null;
            if (data.armed != 0)
            {
                toSend = armOffEvent;
            }
            else if (data.position < 100)
            {
                newPosition = Math.Min(data.position + 25, 100);
            }

            if (toSend != null)
            {
                _logger.LogDebug($"Now send {toSend}");
                simConnect.SendEvent(toSend);
            }

            if (newPosition != null)
            {
                uint eventData = (uint)newPosition * 164;
                eventData = Math.Min(eventData, 16384u);
                _logger.LogDebug($"Now demand position {newPosition} or as U {eventData}");
                simConnect.SendEvent(setEvent, eventData);
            }
        }
    }

    [Component]
    public class LessSpoiler : AbstractSpoilerDataListener
    {
        private readonly SpoilerArmOnEvent armOnEvent;

        public LessSpoiler(IServiceProvider sp) : base(sp)
        {
            armOnEvent = sp.GetRequiredService<SpoilerArmOnEvent>();
        }

        protected override void ProcessSpoilerDemand(ExtendedSimConnect simConnect, SpoilerData data)
        {
            if (data.position > 0)
            {
                int newPosition = Math.Max(data.position - 25, 0);
                uint eventData = (uint)newPosition * 164u;
                eventData = Math.Min(eventData, 16384u);
                _logger.LogDebug($"Now demand position {newPosition} or as U {eventData}");
                simConnect.SendEvent(setEvent, eventData);
            }
            else if (data.armed == 0)
            {
                _logger.LogDebug($"Now send {armOnEvent}");
                simConnect.SendEvent(armOnEvent);
            }
        }
    }
}
