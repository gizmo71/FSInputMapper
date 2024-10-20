using Controlzmo.GameControllers;
using Controlzmo.Hubs;
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
    public partial class FcuSpeedMachToggled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_SPD_MACH_TOGGLE_PUSH";
        public string GetId() => "speedMachToggled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _)
        {
            if (simConnect.IsFenix)
            {
                for (int i = 0; i < 2; ++i)
                    sender.Execute(simConnect, "(L:S_FCU_SPD_MACH) ++ (>L:S_FCU_SPD_MACH)");
            }
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuSpeedPulled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_SPD_PULL";
        public string GetId() => "fcuSpeedPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _)
        {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_SPEED) ++ (>L:S_FCU_SPEED)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuSpeedPushed : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_SPD_PUSH";
        public string GetId() => "fcuSpeedPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _)
        {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_SPEED) -- (>L:S_FCU_SPEED)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class PushPullFcuSpeed : AbstractButtonShortLongPress<UrsaMinorFighterR>
    {
        private readonly FcuSpeedPulled pull;
        private readonly FcuSpeedPushed push;
        public override int GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_ROUND;
        public override void OnLongPress(ExtendedSimConnect simConnect) => pull.SetInSim(simConnect, true);
        public override void OnShortPress(ExtendedSimConnect simConnect) => push.SetInSim(simConnect, true);
    }

    [Component]
    public class FcuSpeedInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_INC";
    }

    [Component]
    public class FcuSpeedDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_DEC";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuSpeedDelta : ISettable<Int16>
    {
        private readonly FcuSpeedInc inc;
        private readonly FcuSpeedDec dec;
        private readonly JetBridgeSender sender;

        private Int32 fenixAdjustment = 0;

        public string GetId() => "fcuSpeedDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                Interlocked.Add(ref fenixAdjustment, value);
                sender.Execute(simConnect, delegate() {
                    var toSend = Interlocked.Exchange(ref fenixAdjustment, 0);
                    var op = toSend < 0 ? "-" : "+";
                    return toSend == 0 ? null : $"(L:E_FCU_SPEED) {Math.Abs(toSend)} {op} (>L:E_FCU_SPEED)";
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
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuSpeedRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuSpeedDelta delta;
        private readonly FcuSpeedMachToggled toggle;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) => toggle.SetInSim(simConnect!, false);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class IncOrToggleFcuSpeed : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly FcuSpeedRepeatingDoublePress controller;
        public int GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_FAR_LEFT_UP;
        public virtual void OnPress(ExtendedSimConnect simConnect) => controller.Press(simConnect, AbstractRepeatingDoublePress.Direction.Up);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => controller.Release(simConnect);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class DecOrToggleFcuSpeed : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly FcuSpeedRepeatingDoublePress controller;
        public int GetButton() => UrsaMinorFighterR.BUTTON_LEFT_BASE_FAR_LEFT_DOWN;
        public virtual void OnPress(ExtendedSimConnect simConnect) => controller.Press(simConnect, AbstractRepeatingDoublePress.Direction.Down);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => controller.Release(simConnect);
    }
}
