using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using CoreDX.vJoy.Wrapper;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using static Controlzmo.GameControllers.IVJoyControllerExtensions;

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
Console.Error.WriteLine($"trim percent {data.fenixDecaUnits} FBW {data.fbwTrim} generic {data.trim}");
            if (simConnect.IsFBW)
                data.fenixDecaUnits = (Int32) (Math.Round(data.fbwTrim * 10.0, MidpointRounding.AwayFromZero));
            else if (!simConnect.IsFenix)
                data.fenixDecaUnits = (Int32) (data.trim * 200.0);
Console.Error.WriteLine($"             -> {data.fenixDecaUnits}");
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
    public partial class RudderTrimKnob
    {
        private readonly JetBridgeSender _sender;
        private readonly VirtualJoy vJoy;
        internal const uint LEFT = 0;
        internal const uint CENTRE = 1;
        internal const uint RIGHT = 2;

        internal void Set(ExtendedSimConnect sc, uint value)
        {
            if (sc.IsIniBuilds)
                _sender.Execute(sc, $"{value} (>L:XMLVAR_RUDDERTRIM_SWITCH_1)");
            else if (sc.IsFenix && false)
// Fenix
//TODO: S_FC_RUDDER_TRIM doesn't work - still needs repeated setting :-(
// RUDDER_TRIM_SET (and _EX1) don't seem to work either. :-(
// L:N_FC_RUDDER_TRIM_DECIMAL can't be meaningfully set.
                _sender.Execute(sc, $"{value} (>L:S_FC_RUDDER_TRIM)");
            else {
                var controller = vJoy.getController();
                PressOrRelease(value == LEFT)(controller, VJoyButton.RUDDER_TRIM_LEFT);
                PressOrRelease(value == RIGHT)(controller, VJoyButton.RUDDER_TRIM_RIGHT);
            }
        }

        private ButtonAction PressOrRelease(bool isPress) => isPress ? IVJoyControllerExtensions.PressButton : IVJoyControllerExtensions.ReleaseButton;
        private delegate bool ButtonAction(IVJoyController controller, VJoyButton button);
    }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimLeft : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimKnob knob;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_LEFT;
        public virtual void OnPress(ExtendedSimConnect sc) => knob.Set(sc, RudderTrimKnob.LEFT);
        public virtual void OnRelease(ExtendedSimConnect sc) => knob.Set(sc, RudderTrimKnob.CENTRE);
    }


    [Component, RequiredArgsConstructor]
    public partial class RudderTrimRight : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimKnob knob;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_RIGHT;
        public virtual void OnPress(ExtendedSimConnect sc) => knob.Set(sc, RudderTrimKnob.RIGHT);
        public virtual void OnRelease(ExtendedSimConnect sc) => knob.Set(sc, RudderTrimKnob.CENTRE);
    }
}
