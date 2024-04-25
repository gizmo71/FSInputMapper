using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Threading;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltPulled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_ALT_PULL";
        public string GetId() => "fcuAltPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) ++ (>L:S_FCU_ALTITUDE)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltPushed : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_ALT_PUSH";
        public string GetId() => "fcuAltPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) -- (>L:S_FCU_ALTITUDE)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    public class FcuAltInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_INC";
    }

    [Component]
    public class FcultDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_DEC";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltDelta : ISettable<Int16>
    {
        private readonly FcuAltInc inc;
        private readonly FcultDec dec;
        private readonly JetBridgeSender sender;

        private Int32 fenixAdjustment = 0;

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                Interlocked.Add(ref fenixAdjustment, value);
                sender.Execute(simConnect, delegate() {
                    var toSend = Interlocked.Exchange(ref fenixAdjustment, 0);
                    var op = toSend < 0 ? "-" : "+";
                    return toSend == 0 ? null : $"(L:E_FCU_ALTITUDE) {Math.Abs(toSend)} {op} (>L:E_FCU_ALTITUDE)";
                });
            }
            else
                while (value != 0)
                {
                    simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltIncrement : ISettable<uint>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string GetId() => "fcuAltIncrement";
        public string SimEvent() => "A32NX.FCU_ALT_INCREMENT_SET";
        public void SetInSim(ExtendedSimConnect simConnect, uint value) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, (value == 1000 ? 1 : 0) + " (>L:S_FCU_ALTITUDE_SCALE)");
            else
                simConnect.SendEvent(this, value);
        }
    }
}
