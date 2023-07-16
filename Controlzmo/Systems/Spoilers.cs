using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using Windows.Gaming.Input;

namespace Controlzmo.Systems.Spoilers
{
    [Component]
    public class Walrus : CreateOnStartup
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly MoreSpoiler moreListener;
        private readonly LessSpoiler lessListener;
        private readonly SimConnectHolder holder;
        private readonly ILogger _logging;
// https://learn.microsoft.com/en-us/windows/uwp/gaming/raw-game-controller
// Will we get inputs it we're not in focus?
        private RawGameController? hotas;

        public Walrus(IServiceProvider sp)
        {
            _logging = sp.GetRequiredService<ILogger<Walrus>>();
            moreListener = sp.GetRequiredService<MoreSpoiler>();
            lessListener = sp.GetRequiredService<LessSpoiler>();
            holder = sp.GetRequiredService<SimConnectHolder>();
            RawGameController.RawGameControllerAdded += Added;
            RawGameController.RawGameControllerRemoved += Removed;
            RawGameController.RawGameControllers.ToList().ForEach(c => Added(null, c));
        }

        private void Removed(object? sender, RawGameController c)
        {
            if (IsHotas(c))
            {
                hotas = null;
                hotasButtons = null;
                _logging.LogWarning($"Removed HOTAS");
                cancellationTokenSource.Cancel();
            }
        }

        private void Added(object? sender, RawGameController c)
        {
            if (IsHotas(c) && hotas == null)
            {
                hotasButtons = new bool[c.ButtonCount];
                hotas = c;
                _logging.LogInformation($"Added HOTAS");
                var token = cancellationTokenSource.Token;
                Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        Poll();
                        Thread.Sleep(1);
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else
                _logging.LogInformation($"Not (new) HOTAS: {c.HardwareProductId} by {c.HardwareVendorId} ({c.DisplayName}) buttons {c.ButtonCount}");
        }

        private static bool IsHotas(RawGameController c)
        {
            // 1031 by 1103 31 butons TCA thrust levers
            //45322 by 1103 16 buttons T16000M stick
            //45845 by 1103 12 buttons Ferrari gamepad
            //46727 by 1103 14 buttons is the HOTAS thingy
            return c.HardwareVendorId == 1103 && c.HardwareProductId == 46727;
        }

        private bool[]? hotasButtons;

        private void Poll()
        {
            var buttonArray = new bool[14];
            var axisArray = new double[0];
            var switchArray = new GameControllerSwitchPosition[0];
            hotas?.GetCurrentReading(buttonArray, switchArray, axisArray);
            for (int i = 0; i < hotas?.ButtonCount; ++i)
            {
                if (buttonArray[i] != hotasButtons?[i])
                {
                    ButtonChange(i, hotasButtons![i] = buttonArray[i]);
                }
            }
        }

        private void ButtonChange(int index, bool value)
        {
            _logging.LogDebug($"Button {index}: {value}");
            if (value == true) // Pressed
            {
                switch (index)
                {
//TODO: what should we do if we're not in the A32NX? Send the toggles as before?
                    case 11: // Forward
                        _logging.LogDebug("User has asked for less speedbrake");
                        holder.SimConnect?.RequestDataOnSimObject(lessListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                        break;
                    case 13: // Backward
                        _logging.LogDebug("User has asked for more speedbrake");
                        holder.SimConnect?.RequestDataOnSimObject(moreListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                        break;
                }
            }
        }
    }
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
    };

    // Output events
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
            _logger.LogDebug($"Wants spoiler; raw data pos {data.position} armed {data.armed} A32NX armed {data.a32nxArmed} active {data.a32nxGroundSpoilersActive} handle {data.a32nxPosition}");
//TODO: only do this if we're in the A32NX? Or will this never trigger?
            if (data.a32nxArmed == 1 || data.a32nxGroundSpoilersActive == 1) data.armed = 1;
//TODO: only do this if we're in the A32NX
            data.position = (int)(100 * data.a32nxPosition);
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
