using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Controlzmo.Systems.Controls
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RudderTrimData
    {
        [SimVar("RUDDER TRIM PCT", "Percent over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.00005f)]
        public float trim;
        [SimVar("L:A32NX_HYD_RUDDER_TRIM_FEEDBACK_ANGLE", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float fbwTrim;
        [SimVar("L:N_FC_RUDDER_TRIM_DECIMAL", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixDecaUnits;
    };

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimDisplay : DataListener<RudderTrimData>, IRequestDataOnOpen
    {
        private readonly UrsaMinorOutputs output;
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, RudderTrimData data)
        {
            if (simConnect.IsFBW)
                data.fenixDecaUnits = (Int32) (Math.Round(data.fbwTrim * 10.0, MidpointRounding.AwayFromZero));
            else if (!simConnect.IsFenix)
                data.fenixDecaUnits = (Int32) (data.trim * 200.0);
            output.SetTrimDisplay(data.fenixDecaUnits);
        }

    }

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
    public partial class RepeatingRudderTrimEvent
    {
        private readonly SimConnectHolder holder;
        private Timer? timer;

        private IEvent? _event;
        internal void Send(IEvent? newEvent) {
            lock(this)
            {
                if (timer == null)
                    timer = new Timer(HandleTimer);
                this._event = newEvent;
            }
//TODO: Even 35 is NOT fast enough in the Fenix, but going faster than 40ms can crash SimConnect. :-(
            var delay = holder.SimConnect?.IsFenix == true ? 40 : 100;
            timer.Change(delay, delay);
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
    [Component]
    public class RudderTrimRightEvent : IEvent { public string SimEvent() => "RUDDER_TRIM_RIGHT"; }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimKnob
    {
        private readonly JetBridgeSender _sender;
        private readonly RepeatingRudderTrimEvent repeat;

        internal void Set(ExtendedSimConnect sc, IEvent? _event)
        {
return; //TODO: is it easier to just map it in game? :-(
            var value = _event switch {
                RudderTrimLeftEvent left => 0,
                RudderTrimRightEvent right => 2,
                _ => 1
            };
            if (sc.IsIniBuilds)
                _sender.Execute(sc, $"{value} (>L:XMLVAR_RUDDERTRIM_SWITCH_1)");
            else if (sc.IsFenix && false)
// Fenix
//TODO: S_FC_RUDDER_TRIM doesn't work - still needs repeated setting :-(
// RUDDER_TRIM_SET (and _EX1) don't seem to work either. :-(
// L:N_FC_RUDDER_TRIM_DECIMAL can't be meaningfully set.
                _sender.Execute(sc, $"{value} (>L:S_FC_RUDDER_TRIM)");
            else
                repeat.Send(_event);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimLeft : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimLeftEvent _event;
        private readonly RudderTrimKnob knob;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_LEFT;
        public virtual void OnPress(ExtendedSimConnect sc) => knob.Set(sc, _event);
        public virtual void OnRelease(ExtendedSimConnect sc) => knob.Set(sc, null);
    }


    [Component, RequiredArgsConstructor]
    public partial class RudderTrimRight : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimRightEvent _event;
        private readonly RudderTrimKnob knob;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_RIGHT;
        public virtual void OnPress(ExtendedSimConnect sc) => knob.Set(sc, _event);
        public virtual void OnRelease(ExtendedSimConnect sc) => knob.Set(sc, null);
    }
}
