using FSInputMapper.Event;

namespace FSInputMapper.Systems.Lights
{

    [Singleton]
    public class SetStrobesEvent : IEvent
    {
        public string SimEvent() { return "STROBES_SET"; }
    }

    [Singleton]
    public class ToggleBeaconLightsEvent : IEvent
    {
        public string SimEvent() { return "TOGGLE_BEACON_LIGHTS"; }
    }

    [Singleton]
    public class ToggleWingIceLightsEvent : IEvent
    {
        public string SimEvent() { return "TOGGLE_WING_LIGHTS"; }
    }

    [Singleton]
    public class SetLogoLightsEvent : IEvent
    {
        public string SimEvent() { return "LOGO_LIGHTS_SET"; }
    }

    [Singleton]
    public class SetNavLightsEvent : IEvent
    {
        public string SimEvent() { return "NAV_LIGHTS_SET"; }
    }

    [Singleton]
    public class SetTaxiLightsEvent : IEvent
    {
        public string SimEvent() { return "TAXI_LIGHTS_SET"; }
    }

}
