using Controlzmo.GameControllers;
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
        public int GetButton() => UrsaMinorFighterR.BUTTON_SMALLER_ROUND;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
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
