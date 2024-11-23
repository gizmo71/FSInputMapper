using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    // We prefer "GEAR_SET", but Fenix doesn't support it. :-(
    [Component] public class GearUpEvent : IEvent { public string SimEvent() => "GEAR_UP"; }
    [Component] public class GearDownEvent : IEvent { public string SimEvent() => "GEAR_DOWN"; }

    [Component, RequiredArgsConstructor]
    public partial class LandingGearHandle : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly GearUpEvent _up;
        private readonly GearDownEvent _down;

        public int GetAxis() => UrsaMinorFighterR.AXIS_THROTTLE;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            if (@new < 0.25 && old >= 0.25)
                sc.SendEvent(_up);
            else if (@new > 0.75 && old <= 0.75)
                sc.SendEvent(_down);
        }
    }
}
