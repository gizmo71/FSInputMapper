using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Lights
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
//TODO: try using BEACON_LIGHTS_SET, with two parameters, the state 0/1 and the light index
            sender.Execute(simConnect, $"(A:LIGHT BEACON, Bool) {desiredValue} != if{{ 0 (>K:TOGGLE_BEACON_LIGHTS) }}");
        }
    }

    [Component]
    public class ToggleBeaconLightsEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_BEACON_LIGHTS";
    }

    [Component]
    public class BeaconLightsSetter : ISettable<bool>
    {
        private readonly ToggleBeaconLightsEvent toggleBeaconLightsEvent;

        public BeaconLightsSetter(ToggleBeaconLightsEvent toggleBeaconLightsEvent)
        {
            this.toggleBeaconLightsEvent = toggleBeaconLightsEvent;
        }

        public string GetId() => "lightsBeacon";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            simConnect.SendEvent(toggleBeaconLightsEvent);
        }
    }
}
