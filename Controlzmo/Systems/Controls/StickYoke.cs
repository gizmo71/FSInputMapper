using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetAileronsEvent : IEvent { public string SimEvent() => "AXIS_AILERONS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Ailerons : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetAileronsEvent _event;

        public int GetAxis() => UrsaMinorFighterR.AXIS_ROLL;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => sc.SendEvent(_event, 16383 - (int) (32767.0 * @new));
    }

    [Component] public class SetElevatorsEvent : IEvent { public string SimEvent() => "AXIS_ELEVATOR_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Elevators : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetElevatorsEvent _event;

        public int GetAxis() => UrsaMinorFighterR.AXIS_PITCH;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => sc.SendEvent(_event, 16383 - (int) (32767.0 * @new));
    }
}
