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
            var eventCode = value switch { -1 => "A32NX.FCU_ALT_DEC", 1 => "A32NX.FCU_ALT_INC", _ => throw new Exception($"Cannot FCU alt change by {value}") };
            sender.Execute(simConnect, $"0 (>K:{eventCode})");
        }
    }
}
