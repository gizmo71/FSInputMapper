using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Reflection;
using System.Threading;
using static Controlzmo.GameControllers.AbstractRepeatingDoublePress;

namespace Controlzmo.Systems.Controls
{

    [Component]
    public class RudderTrimResetEvent : IEvent { public string SimEvent() => "RUDDER_TRIM_RESET"; }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimReset : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimResetEvent _event;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_RESET;
        public virtual void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
    }

    [Component, RequiredArgsConstructor]
    public partial class RepeatingRudderTrimEvent : CreateOnStartup
    {
        private readonly SimConnectHolder holder;
        private Timer? timer;

        private IEvent? _event;
        internal void Send (IEvent? newEvent) {
            lock(this)
            {
                if (timer == null)
                    timer = new Timer(HandleTimer);
                this._event = newEvent;
            }
            timer.Change(100, 100);
        }

        private void HandleTimer(object? _) {
            lock (this) {
                if (_event == null) timer!.Change(Timeout.Infinite, Timeout.Infinite);
                else holder.SimConnect?.SendEvent(_event);
            }
        }
    }

    [Component]
    public class RudderTrimLeftEvent : IEvent { public string SimEvent() => "RUDDER_TRIM_LEFT"; }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimLeft : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimLeftEvent _event;
        private readonly RepeatingRudderTrimEvent repeat;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_LEFT;
        public virtual void OnPress(ExtendedSimConnect _) => repeat.Send(_event);
        public virtual void OnRelease(ExtendedSimConnect _) => repeat.Send(null);
    }

    [Component]
    public class RudderTrimRightEvent : IEvent { public string SimEvent() => "RUDDER_TRIM_RIGHT"; }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimRight : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimRightEvent _event;
        private readonly RepeatingRudderTrimEvent repeat;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_RIGHT;
        public virtual void OnPress(ExtendedSimConnect _) => repeat.Send(_event);
        public virtual void OnRelease(ExtendedSimConnect _) => repeat.Send(null);
    }
}
