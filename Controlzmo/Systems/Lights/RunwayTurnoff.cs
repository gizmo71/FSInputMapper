using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class TaxiLightSetEvent : IEvent
    {
        public string SimEvent() => "TAXI_LIGHTS_SET";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class RunwayTurnoffLightSystem : ISettable<bool>
    {
        private readonly TaxiLightSetEvent rtEvent;
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsRunwayTurnoff";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            uint code = value ? 1u : 0u;
            if (simConnect.IsFBW) {
                simConnect.SendEventEx1(rtEvent, code, 2);
                simConnect.SendEventEx1(rtEvent, code, 3);
            }
            else if (simConnect.IsFenix)
            {
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_RWY_TURNOFF)");
            }
        }
    }
}
