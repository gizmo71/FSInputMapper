using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.FlightControlUnit
{
    public class FcuAltMode : LVar, IOnSimConnection
    {
        private readonly SerialPico serial;

        public FcuAltMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_FCU_ALT_MANAGED";
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine(Value switch { 0 => "A", 1 => "a", _ => $"=Unknown FCU alt {Value}" });
    }

    [Component]
    public class fcuAltPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public fcuAltPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_ALT_PULL)");
    }

    [Component]
    public class fcuAltPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public fcuAltPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_ALT_PUSH)");
    }

    [Component]
    public class FcuAltDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuAltDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_ALT_DEC" : "A32NX.FCU_ALT_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}
