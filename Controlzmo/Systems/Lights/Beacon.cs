using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
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
        private readonly JetBridgeSender sender;

        public string GetId() => "beaconLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value) {
            var code = value == true ? 1u : 0u;
            if (simConnect.IsFBW)
                simConnect.SendEvent(setEvent, code);
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_BEACON)");
            else if (simConnect.IsIni320 || simConnect.IsIni321)
                sender.Execute(simConnect, $"{code} (>L:INI_BEACON_LIGHT_SWITCH)");
        }
    }
}
