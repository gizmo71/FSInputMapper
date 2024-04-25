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
    public partial class FcuHeadingPulled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_HDG_PULL";
        public string GetId() => "fcuHeadingPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) ++ (>L:S_FCU_HEADING)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuHeadingPushed : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_HDG_PUSH";
        public string GetId() => "fcuHeadingPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_HEADING) -- (>L:S_FCU_HEADING)");
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
                    simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
            }
        }
    }
}
