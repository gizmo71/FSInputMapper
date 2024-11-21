using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetGearEvent : IEvent { public string SimEvent() => "GEAR_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class LandingGearHandle : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetGearEvent _event;

        public int GetAxis() => UrsaMinorFighterR.AXIS_THROTTLE;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            uint value;
            if (@new < 0.25 && old >= 0.25)
                value = 0; // Up
            else if (@new > 0.75 && old <= 0.75)
                value = 1; // Down
            else
                return;
            sc.SendEvent(_event, value);
        }
    }
}
