using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component] public class AutopilotOnEvent : IEvent { public string SimEvent() => "AUTOPILOT_ON"; }
    [Component] public class AutopilotOffEvent : IEvent { public string SimEvent() => "AUTOPILOT_OFF"; }

    [Component, RequiredArgsConstructor]
    public partial class AutopilotOn : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly AutopilotOnEvent _event;
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_SMALLER_ROUND;

        public void OnPress(ExtendedSimConnect sc) {
            if (sc.IsA32NX || sc.IsA339)
                sender.Execute(sc, "(L:A32NX_AUTOPILOT_1_ACTIVE) if{ (>K:A32NX.FCU_AP_2_PUSH) } els{ (>K:A32NX.FCU_AP_1_PUSH) }");
            else if (sc.IsIni330)
                sender.Execute(sc, "1 (L:INI_ap1_on) if{ (>L:INI_AP2_BUTTON) } els{ (>L:INI_AP1_BUTTON) }");
            else
                sc.SendEvent(_event);
        }
    }

    [Component] public class LessFlapEvent : IEvent { public string SimEvent() => "FLAPS_DECR"; }

    [Component, RequiredArgsConstructor]
    public partial class AutopilotOff : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly AutopilotOffEvent _event;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RED;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
    }
}
