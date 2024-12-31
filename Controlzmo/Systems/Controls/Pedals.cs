using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetRudderEvent : IEvent { public string SimEvent() => "AXIS_RUDDER_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Rudder : IAxisCallback<T16000mHotas>
    {
        private readonly SetRudderEvent _event;
        private readonly AxisSetter setter;
        public int GetAxis() => T16000mHotas.AXIS_RUDDER_PEDALS;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.Set(sc, _event, @new);
    }

    [Component] public class SetBrakeLeftEvent : IEvent { public string SimEvent() => "AXIS_LEFT_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class LeftToeBrake : IAxisCallback<T16000mHotas>
    {
        private readonly SetBrakeLeftEvent _event;
        private readonly AxisSetter setter;

        public int GetAxis() => T16000mHotas.AXIS_TOE_BRAKE_LEFT;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.Set(sc, _event, @new);
    }

    [Component] public class SetBrakeRightEvent : IEvent { public string SimEvent() => "AXIS_RIGHT_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class RightToeBrake : IAxisCallback<T16000mHotas>
    {
        private readonly SetBrakeRightEvent _event;
        private readonly AxisSetter setter;

        public int GetAxis() => T16000mHotas.AXIS_TOE_BRAKE_RIGHT;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.Set(sc, _event, @new);
    }
}
