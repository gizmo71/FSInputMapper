using Controlzmo.Hubs;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class BeaconLightSetEvent : IEvent
    {
        public string SimEvent() => "BEACON_LIGHTS_SET";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BeaconLight : ISettable<bool?>
    {
        private readonly BeaconLightSetEvent setEvent;

        public string GetId() => "beaconLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value) => simConnect.SendEvent(setEvent, value == true ? 1u : 0u);
    }
}
