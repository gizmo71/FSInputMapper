using System;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class fcuAltPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public fcuAltPushed(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "fcuAltPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            sender.Execute(simConnect, $"(>K:A32NX.FCU_ALT_PUSH)");
        }
    }

    [Component]
    public class FcuAltDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuAltDelta(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_ALT_DEC" : "A32NX.FCU_ALT_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short) Math.Sign(value);
            }
        }
    }
}
