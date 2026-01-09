using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetParkingBrakeEvent : IEvent { public string SimEvent() => "PARKING_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBrakeSet : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly SetParkingBrakeEvent _event;
        public int GetButton() => UrsaMinorThrottle.BUTTON_PARKING_BRAKE_SET;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event, 1);
    }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBrakeRelease : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly SetParkingBrakeEvent _event;
        public int GetButton() => UrsaMinorThrottle.BUTTON_PARKING_BRAKE_RELEASED;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event, 0);
    }
}
