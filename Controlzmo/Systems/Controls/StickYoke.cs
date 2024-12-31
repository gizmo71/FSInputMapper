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
        private readonly AxisSetter setter;
        public int GetAxis() => UrsaMinorFighterR.AXIS_ROLL;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.SetAvoidingCentreRepetition(sc, _event, old, @new, 0.505);
    }

    [Component] public class SetElevatorsEvent : IEvent { public string SimEvent() => "AXIS_ELEVATOR_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Elevators : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetElevatorsEvent _event;
        private readonly AxisSetter setter;
        public int GetAxis() => UrsaMinorFighterR.AXIS_PITCH;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.SetAvoidingCentreRepetition(sc, _event, old, @new, 0.5);
    }
}
