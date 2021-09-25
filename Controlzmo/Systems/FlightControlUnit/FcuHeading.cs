using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

/*TODO: Display data is a bit complex.
We have LVars A32NX_FCU_HDG_MANAGED_DASHES/A32NX_FCU_HDG_MANAGED_DOT, which are 0/1 booleans,
and A32NX_AUTOPILOT_HEADING_SELECTED (in Degrees), instantly updated; -1 if managed heading mode.
But remember that a value may be shown before switch from managed to selected. */
namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class fcuHeadingPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public fcuHeadingPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuHeadingPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_HDG_PULL)");
    }

    [Component]
    public class fcuHeadingPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public fcuHeadingPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuHeadingPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_HDG_PUSH)");
    }

    [Component]
    public class FcuHeadingDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuHeadingDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_HDG_DEC" : "A32NX.FCU_HDG_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}
