using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;

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

        public string GetId() => "fcuSpeedDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                var op = value < 0 ? "-" : "+";
                sender.Execute(simConnect, $"(L:E_FCU_SPEED) {Math.Abs(value)} {op} (>L:E_FCU_SPEED)");
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
}
