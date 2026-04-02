using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingPulled : ISettable<bool>, IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_NEAR_DOWN;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);

        public string SimEvent() => "A32NX.FCU_HDG_PULL";
        public string GetId() => "DISABLEDfcuHeadingPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) ++ (>L:S_FCU_HEADING)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_SELECTED_HEADING_BUTTON)");
            else if (simConnect.IsAtr7x)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_HDG)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingPushed : ISettable<bool>, IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_NEAR_UP;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);

        public string SimEvent() => "A32NX.FCU_HDG_PUSH";
        public string GetId() => "DISABLEDfcuHeadingPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) -- (>L:S_FCU_HEADING)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_MANAGED_HEADING_BUTTON)");
            else if (simConnect.IsAtr7x)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_NAV)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    public class FcuHeadingInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_INC";
    }

    [Component]
    public class FcuHeadingDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_DEC";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingDelta : ISettable<Int16>
    {
        private readonly FcuHeadingInc inc;
        private readonly FcuHeadingDec dec;
        private readonly JetBridgeSender sender;
        private readonly InputEvents inputEvents;

        private Int32 lvarAdjustment = 0;

        public string GetId() => "DISABLEDfcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix || simConnect.IsAtr7x) {
                Interlocked.Add(ref lvarAdjustment, value);
                sender.Execute(simConnect, ExecuteLvar);
            }
            else
            {
                while (value != 0)
                {
                    if (simConnect.IsIni330)
                        inputEvents.Send(simConnect, "AIRLINER_MCU_HDG", (double) Math.Sign(value));
                    else if (simConnect.IsIniBuilds)
                        inputEvents.Send(simConnect, "INSTRUMENT_FCU_HDG_KNOB", (double) Math.Sign(value));
                    else
                        simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
            }
        }

        private String? ExecuteLvar(ExtendedSimConnect simConnect)
        {
            string lvar = "E_FCU_HEADING";
            string extra = "";
            if (simConnect.IsAtr7x)
            {
                lvar = "MSATR_SEL_HDG";
                extra = " 1 (>L:MSATR_SEL_HDG_CHANGED)";
            }

                var toSend = Interlocked.Exchange(ref lvarAdjustment, 0);
            var op = toSend < 0 ? "-" : "+";
            return toSend == 0 ? null : $"(L:{lvar}) {Math.Abs(toSend)} {op} dnor (>L:{lvar}){extra}";
        }
    }


    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuHeadingDelta delta;
        private readonly FcuTrackFpaToggled toggle;
        private readonly JetBridgeSender sender;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) {
            if (simConnect?.IsAtr7x == true)
                sender.Execute(simConnect, "1 (>L:MSATR_SEL_HDG_SYNC)");
            else
                toggle.SetInSim(simConnect!, true);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class IncOrToggleFcuHdg : RepeatingDoublePressButton<UrsaMinorFighterR, FcuHeadingRepeatingDoublePress>
    {
        [Property]
        private readonly FcuHeadingRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_FAR_RIGHT_UP;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuHeadingRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Up;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class DecOrToggleFcuHdg : RepeatingDoublePressButton<UrsaMinorFighterR, FcuHeadingRepeatingDoublePress>
    {
        [Property]
        private readonly FcuHeadingRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_FAR_RIGHT_DOWN;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuHeadingRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Down;
    }
}
