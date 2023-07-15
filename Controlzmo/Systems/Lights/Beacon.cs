using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class BeaconLightSetEvent : IEvent
    {
        public string SimEvent() => "BEACON_LIGHTS_SET";
    }

    [Component]
    public class BeaconLight : ISettable<bool?>
    {
        private readonly BeaconLightSetEvent @event;

        public BeaconLight(IServiceProvider sp) => this.@event = sp.GetRequiredService<BeaconLightSetEvent>();

        public string GetId() => "beaconLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value) => simConnect.SendEvent(@event, value == true ? 1u : 0u);
    }
}
