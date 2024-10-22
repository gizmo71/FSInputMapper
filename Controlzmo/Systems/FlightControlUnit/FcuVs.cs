using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Threading;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuVsPulled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_VS_PULL";
        public string GetId() => "fcuVsPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_VERTICAL_SPEED) ++ (>L:S_FCU_VERTICAL_SPEED)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuVsPushed : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_VS_PUSH";
        public string GetId() => "fcuVsPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_VERTICAL_SPEED) -- (>L:S_FCU_VERTICAL_SPEED)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class PushPullFcuVs : AbstractButtonShortLongPress<UrsaMinorFighterR>
    {
        private readonly FcuVsPulled pull;
        private readonly FcuVsPushed push;
        public override int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_ROUND;
        public override void OnLongPress(ExtendedSimConnect simConnect) => pull.SetInSim(simConnect, true);
        public override void OnShortPress(ExtendedSimConnect simConnect) => push.SetInSim(simConnect, true);
    }

    [Component]
    public class FcuVsInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_INC";
    }

    [Component]
    public class FcuVsDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_DEC";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuVsDelta : ISettable<Int16>
    {
        private readonly FcuVsInc inc;
        private readonly FcuVsDec dec;
        private readonly JetBridgeSender sender;

        private Int32 fenixAdjustment = 0;

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                Interlocked.Add(ref fenixAdjustment, value);
                sender.Execute(simConnect, delegate() {
                    var toSend = Interlocked.Exchange(ref fenixAdjustment, 0);
                    var op = toSend < 0 ? "-" : "+";
                    return toSend == 0 ? null : $"(L:E_FCU_VS) {Math.Abs(toSend)} {op} (>L:E_FCU_VS)";
                });
            }
            else
            {
                while (value != 0)
                {
                    simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
            }
//TODO: in the real FCU, when turning quickly, it takes *two* clicks to change by 100 ft/min V/S.
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuVsRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuVsDelta delta;
        private readonly FcuTrackFpaToggled toggle;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) => toggle.SetInSim(simConnect!, false);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class IncOrToggleFcuVs : RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>
    {
        [Property]
        private readonly FcuVsRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_RIGHT_UP;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Up;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class DecOrToggleFcuVs : RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>
    {
        [Property]
        private readonly FcuVsRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_RIGHT_DOWN;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuVsRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Down;
    }
}
