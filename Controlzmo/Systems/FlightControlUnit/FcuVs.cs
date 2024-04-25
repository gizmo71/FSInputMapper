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
}
