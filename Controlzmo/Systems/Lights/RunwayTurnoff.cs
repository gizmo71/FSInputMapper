using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class TaxiLightSetEvent : IEvent
    {
        public string SimEvent() => "TAXI_LIGHTS_SET";
    }

    [Component]
    public class RunwayTurnoffLightSystem : ISettable<bool>
    {
        private readonly TaxiLightSetEvent rtEvent;

        public RunwayTurnoffLightSystem(TaxiLightSetEvent rtEvent)
        {
            this.rtEvent = rtEvent;
        }

        public string GetId() => "lightsRunwayTurnoff";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            uint code = value ? 1u : 0u;
            simConnect.SendEvent(rtEvent, code, 2);
            simConnect.SendEvent(rtEvent, code, 3);
        }
    }
}
