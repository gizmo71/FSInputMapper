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
            if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_BEACON)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{code} (>L:INI_BEACON_LIGHT_SWITCH)");
            else if (simConnect.IsAtr7x)
                sender.Execute(simConnect, $"{code} (>L:MSATR_ELTS_BEACON)");
            else
                simConnect.SendEvent(setEvent, code);
        }
    }
}
