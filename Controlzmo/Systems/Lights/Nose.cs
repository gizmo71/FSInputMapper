using System;
using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class LandingLightSetEvent : IEvent
    {
        public string SimEvent() => "LANDING_LIGHTS_SET";
    }

    [Component]
    public class NoseLightSystem : ISettable<string?>
    {
        private readonly LandingLightSetEvent landingLightEvent;
        private readonly TaxiLightSetEvent taxiLightEvent;

        public NoseLightSystem(LandingLightSetEvent llEvent, TaxiLightSetEvent rtEvent)
        {
            this.taxiLightEvent = rtEvent;
            this.landingLightEvent = llEvent;
        }

        public string GetId() => "lightsNose";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
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
    }
}
