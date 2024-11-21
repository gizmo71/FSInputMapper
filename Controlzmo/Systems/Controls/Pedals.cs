using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component]
    public class ToeBrakeSetter
    {
        internal void Set(ExtendedSimConnect sc, IEvent @event, double value)
        {
            /* Note that the brakes are set on a non-linear scale:
                 -16383 = 0% - is this really not -16384?
                  -8191 = 8%
                      0 = 27%
                  +8191 = 53%
                 +16383 = 100% */
            var mapped = 16383 - (int) (32768.0 * value);
            sc.SendEvent(@event, mapped);
        }
    }

    [Component] public class SetBrakeLeftEvent : IEvent { public string SimEvent() => "AXIS_LEFT_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class LeftToeBrake : IAxisCallback<T16000mHotas>
    {
        private readonly SetBrakeLeftEvent _event;
        private readonly ToeBrakeSetter setter;

        public int GetAxis() => T16000mHotas.AXIS_TOE_BRAKE_LEFT;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.Set(sc, _event, @new);
    }

    [Component] public class SetBrakeRightEvent : IEvent { public string SimEvent() => "AXIS_RIGHT_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class RightToeBrake : IAxisCallback<T16000mHotas>
    {
        private readonly SetBrakeRightEvent _event;
        private readonly ToeBrakeSetter setter;

        public int GetAxis() => T16000mHotas.AXIS_TOE_BRAKE_RIGHT;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.Set(sc, _event, @new);
    }
}
