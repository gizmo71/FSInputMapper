using System;
using System.Runtime.InteropServices;
using Controlzmo.SimConnectzmo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Spoilers
{
/*A32NX rules:
* You cannot arm the spoilers unless the handle is RETRACTED.
* If the new position is anything but that, the arming state is false.
* Ground spoilers can be active during RTO even if not armed, in which case handle position becomes FULL.
* Similar but more complex rules during landing, and position can be PARTIAL without showing active.
* The "SPOILERS HANDLE POSITION" is the sim position, NOT the visible position, A32NX_SPOILERS_HANDLE_POSITION (0-1).
*/
    // Input events
    [Component] public class LessSpoilerArmGroundEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_TOGGLE"; }
    [Component] public class MoreSpoilerToggleEvent : IEvent { public string SimEvent() => "SPOILERS_TOGGLE"; }

    [Component]
    public class MoreSpoilerEventHandler : IEventNotification
    {
        private readonly MoreSpoilerToggleEvent trigger;
        private readonly MoreSpoiler listener;
        private readonly ILogger _logger;

        public MoreSpoilerEventHandler(IServiceProvider sp)
        {
            trigger = sp.GetRequiredService<MoreSpoilerToggleEvent>();
            listener = sp.GetRequiredService<MoreSpoiler>();
            _logger = sp.GetRequiredService<ILogger<MoreSpoilerEventHandler>>();
        }

        public IEvent GetEvent() => trigger;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            _logger.LogTrace("User has asked for more speedbrake (using the toggle command)");
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }
    }

    [Component]
    public class LessSpoilerEventHandler : IEventNotification
    {
        private readonly LessSpoilerArmGroundEvent trigger;
        private readonly LessSpoiler listener;
        private readonly ILogger<LessSpoilerEventHandler> _logger;

        public LessSpoilerEventHandler(IServiceProvider sp)
        {
            trigger = sp.GetRequiredService<LessSpoilerArmGroundEvent>();
            listener = sp.GetRequiredService<LessSpoiler>();
            _logger = sp.GetRequiredService<ILogger<LessSpoilerEventHandler>>();
        }

        public IEvent GetEvent() => trigger;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            _logger.LogTrace("User has asked for less speedbrake (using the toggle command)");
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [SimVar("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 armed;
        [SimVar("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 position;
    };

    // Output events
    [Component] public class SpoilerArmOnEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_ON"; }
    [Component] public class SpoilerArmOffEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_OFF"; }
    [Component] public class SetSpoilerHandleEvent : IEvent { public string SimEvent() => "SPOILERS_SET"; }

    [Component]
    public class A32nxSpoilersArmed : LVar, IOnSimConnection
    {
        public A32nxSpoilersArmed(IServiceProvider sp) : base(sp) { }

        protected override string LVarName() => "A32NX_SPOILERS_ARMED";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component]
    public class A32nxSpoilersActive : LVar, IOnSimConnection
    {
        public A32nxSpoilersActive(IServiceProvider sp) : base(sp) { }

        protected override string LVarName() => "A32NX_SPOILERS_GROUND_SPOILERS_ACTIVE";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component]
    public class A32nxSpoilerHandle : LVar, IOnSimConnection
    {
        public A32nxSpoilerHandle(IServiceProvider sp) : base(sp) { }

        protected override string LVarName() => "A32NX_SPOILERS_HANDLE_POSITION";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    public abstract class AbstractSpoilerDataListener : DataListener<SpoilerData>
    {
        private readonly A32nxSpoilersArmed armed;
        private readonly A32nxSpoilersActive active;
        private readonly A32nxSpoilerHandle handle;
        protected readonly SetSpoilerHandleEvent setEvent;
        protected readonly ILogger _logger;

        protected AbstractSpoilerDataListener(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName);
            armed = sp.GetRequiredService<A32nxSpoilersArmed>();
            active = sp.GetRequiredService<A32nxSpoilersActive>();
            handle = sp.GetRequiredService<A32nxSpoilerHandle>();
            setEvent = sp.GetRequiredService<SetSpoilerHandleEvent>();
        }

        public override void Process(ExtendedSimConnect simConnect, SpoilerData data)
        {
            _logger.LogDebug($"Wants spoiler; raw data pos {data.position} armed {data.armed} A32NX armed {(double?)armed} active {(double?)active} handle {(double?)handle}");
            if ((double?)armed == 1.0f || (double?)active == 1.0f) data.armed = 1;
            var a32nxHandle = (double?)handle;
            if (a32nxHandle is not null) data.position = (int)(100f * a32nxHandle);
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
                //newPosition = 0;
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
