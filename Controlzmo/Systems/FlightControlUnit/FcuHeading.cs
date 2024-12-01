using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;
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
        public string GetId() => "fcuHeadingPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) ++ (>L:S_FCU_HEADING)");
            else if (simConnect.IsIni320 || simConnect.IsIni321)
                sender.Execute(simConnect, "1 (>L:INI_FCU_SELECTED_HEADING_BUTTON)");
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
        public string GetId() => "fcuHeadingPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) -- (>L:S_FCU_HEADING)");
            else if (simConnect.IsIni320 || simConnect.IsIni321)
                sender.Execute(simConnect, "1 (>L:INI_FCU_MANAGED_HEADING_BUTTON)");
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

        private Int32 fenixAdjustment = 0;

        public string GetId() => "fcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                Interlocked.Add(ref fenixAdjustment, value);
                sender.Execute(simConnect, delegate() {
                    var toSend = Interlocked.Exchange(ref fenixAdjustment, 0);
                    var op = toSend < 0 ? "-" : "+";
                    return toSend == 0 ? null : $"(L:E_FCU_HEADING) {Math.Abs(toSend)} {op} (>L:E_FCU_HEADING)";
                });
            }
            else
            {
                while (value != 0)
                {
                    if (simConnect.IsIni320 || simConnect.IsIni321)
                        inputEvents.Send(simConnect, "INSTRUMENT_FCU_HDG_KNOB", (double) Math.Sign(value));
                    else
                        simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
            }
        }
    }


    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuHeadingDelta delta;
        private readonly FcuTrackFpaToggled toggle;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) => toggle.SetInSim(simConnect!, true);
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
