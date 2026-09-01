using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Threading;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component, RequiredArgsConstructor]
    public partial class FcuVsPulled : IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_VS_PULL";

        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_VERTICAL_SPEED) ++ (>L:S_FCU_VERTICAL_SPEED)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:AP9_BUTTON)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_VS)");
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, "(>B:AUTOPILOT_VS_MODE_ON)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FcuVsPushed : IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_VS_PUSH";

        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_VERTICAL_SPEED) -- (>L:S_FCU_VERTICAL_SPEED)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_PUSH_COMMAND)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_VS) 50 (>L:MSATR_FCGP_PITCH_WHEEL)");
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, "1 (>L:AP_ALT_HOLD_ACTIVE) 0 (>L:AP_VS_ACTIVE)"); // Might be a B: event, too...
            else
                simConnect.SendEvent(this);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class PushPullFcuVs : AbstractButtonShortLongPress<UrsaMinorFighterR>
    {
        private readonly FcuVsPulled pull;
        private readonly FcuVsPushed push;
        public override int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_ROUND;
        public override void OnLongPress(ExtendedSimConnect simConnect) => pull.SetInSim(simConnect, true);
        public override void OnShortPress(ExtendedSimConnect simConnect) => push.SetInSim(simConnect, true);
    }

    [Component]
    public class FcuVsInc : IEvent { public string SimEvent() => "A32NX.FCU_VS_INC"; }

    [Component]
    public class FcuVsDec : IEvent { public string SimEvent() => "A32NX.FCU_VS_DEC"; }

    [Component, RequiredArgsConstructor]
    public partial class FcuVsDelta
    {
        private readonly FcuVsInc inc;
        private readonly FcuVsDec dec;
        private readonly JetBridgeSender sender;
        private readonly InputEvents inputEvents;
        private readonly FcuDisplayTopRight trkFpaHolder;

        private Int32 lvarAdjustment = 0;

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix || simConnect.IsAtr || simConnect.IsB78x) {
                Interlocked.Add(ref lvarAdjustment, value);
                sender.Execute(simConnect, ExecuteLvar);
            }
            else
            {
                while (value != 0)
                {
                    if (simConnect.IsIni330)
                        inputEvents.Send(simConnect, "AIRLINER_MCU_VS", (double) Math.Sign(value));
                    else if (simConnect.IsIniBuilds)
                        inputEvents.Send(simConnect, "INSTRUMENT_FCU_VS_KNOB", (double) Math.Sign(value));
                    else
                        simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
            }
        }

        private String? ExecuteLvar(ExtendedSimConnect simConnect)
        {
            var toSend = Interlocked.Exchange(ref lvarAdjustment, 0);
            if (toSend == 0) return null;

            if (simConnect.IsB78x)
                return trkFpaHolder.IsFpa
                    ? $"(L:WT_AP_FPA_Target:1, degree) 0.1 {toSend} * + 9.9 min -9.9 max (>L:WT_AP_FPA_Target:1, degree)"
                    : $"1 (A:AUTOPILOT VERTICAL HOLD VAR:1, feet per minute) 100 {toSend} * + (>K:2:AP_VS_VAR_SET_ENGLISH)";
            var lvar = simConnect.IsAtr ? "MSATR_FCGP_PITCH_WHEEL_DELTA" : "E_FCU_VS";
            return $"(L:{lvar}) {toSend} + (>L:{lvar})";
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FcuVsRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuVsDelta delta;
        private readonly FcuTrackFpaToggled toggle;
        private readonly JetBridgeSender sender;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);

        protected override void BothAction(ExtendedSimConnect? simConnect)
        {
            if (simConnect == null)
                ;
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_IAS)");
            else
                toggle.Toggle(simConnect, false);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class IncOrToggleFcuVs : RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>
    {
        [Property]
        private readonly FcuVsRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_RIGHT_UP;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Up;
    }

    [Component, RequiredArgsConstructor]
    public partial class DecOrToggleFcuVs : RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>
    {
        [Property]
        private readonly FcuVsRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_RIGHT_DOWN;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Down;
    }
}
