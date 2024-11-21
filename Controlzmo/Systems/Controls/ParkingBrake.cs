using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetParkingBrakeEvent : IEvent { public string SimEvent() => "PARKING_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBrake : IAxisCallback<T16000mHotas>
    {
        private readonly SetParkingBrakeEvent _event;

        public int GetAxis() => T16000mHotas.AXIS_THROTTLE;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            uint value;
            if (@new > 0.75 && old <= 0.75)
                value = 0;
            else if (@new < 0.25 && old >= 0.25)
                value = 1;
            else
                return;
            sc.SendEvent(_event, value);
        }
    }
}
