using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuVsMode : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public FcuVsMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_FCU_VS_MANAGED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("FcuVsManaged=" + (Value == 1));
    }

    [Component]
    public class FcuVsPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PULL)");
    }

    [Component]
    public class FcuVsPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PUSH)");
    }

    [Component]
    public class FcuVsDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuVsDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_VS_DEC" : "A32NX.FCU_VS_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}
