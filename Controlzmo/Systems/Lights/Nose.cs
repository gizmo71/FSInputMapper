using System;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class LandingLightSetEvent : IEvent
    {
        public string SimEvent() => "LANDING_LIGHTS_SET";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class NoseLightSystem : ISettable<string?>
    {
        private readonly LandingLightSetEvent landingLightEvent;
        private readonly TaxiLightSetEvent taxiLightEvent;
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsNose";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            if (simConnect.IsFBW)
            {
                uint taxi = 1u;
                uint landing = 0u;
                if (value == "off")
                    taxi = 0u;
                else if (value == "takeoff")
                    landing = 1u;
                else if (value != "taxi")
                    throw new ArgumentException($"Unknown nose light value '{value}'");

                simConnect.SendEventEx1(taxiLightEvent, taxi, 1);
                simConnect.SendEventEx1(landingLightEvent, landing, 1);
            }
            else if (simConnect.IsFenix)
            {
                uint code = value switch { "takeoff" => 2u, "taxi" => 1u, _ => 0u };
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_NOSE)");
            }
        }
    }
}
