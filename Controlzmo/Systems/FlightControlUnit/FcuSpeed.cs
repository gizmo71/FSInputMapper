﻿using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

/*TODO: Display data is a bit complex.
We have LVars A32NX_FCU_SPD_MANAGED_DASHES/A32NX_FCU_SPD_MANAGED_DOT, which are 0/1 booleans, and
A32NX_AUTOPILOT_SPEED_SELECTED, instantly updated: 100 to 399 (knots), 0.10 to 0.99 (M), or -1 for managed.
But remember that a value may be shown before switch from managed to selected. */
namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuSpeedPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_SPD_PULL)");
    }

    [Component]
    public class FcuSpeedPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_SPD_PUSH)");
    }

    [Component]
    public class FcuSpeedDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_SPD_DEC" : "A32NX.FCU_SPD_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}