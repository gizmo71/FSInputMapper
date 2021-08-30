using System;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class BeaconLight : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public BeaconLight(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "beaconLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, $"(A:LIGHT BEACON, Bool) {desiredValue} = if {{ 0 (>K:TOGGLE_BEACON_LIGHTS) }}");
        }
    }

    [Component]
    public class WingIceLight : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public WingIceLight(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "wingIceLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, $"(A:LIGHT WING, Bool) {desiredValue} = if {{ 0 (>K:TOGGLE_WING_LIGHTS) }}");
        }
    }
}
