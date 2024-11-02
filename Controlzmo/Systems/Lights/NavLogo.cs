using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class SetNavLightsEvent : IEvent
    {
        public string SimEvent() => "NAV_LIGHTS_SET";
    }

    [Component]
    public class SetLogoLightsEvent : IEvent
    {
        public string SimEvent() => "LOGO_LIGHTS_SET";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LogoNavLightsSetter : ISettable<bool>
    {
        private readonly SetLogoLightsEvent setLogoLightsEvent;
        private readonly SetNavLightsEvent setNavLightsEvent;
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsNavLogo";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            uint state = value ? 1u : 0u;
            if (simConnect.IsFBW) {
                simConnect.SendEvent(setLogoLightsEvent, state);
                simConnect.SendEvent(setNavLightsEvent, state);
            }
            else if (simConnect.IsFenix)
            {
                sender.Execute(simConnect, $"{state} (>L:S_OH_EXT_LT_NAV_LOGO)");
            }
        }
    }
}
