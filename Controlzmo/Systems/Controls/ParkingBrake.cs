using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
using Lombok.NET;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetParkingBrakeEvent : IEvent { public string SimEvent() => "PARKING_BRAKE_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBreakRouter
    {
        private readonly InputEvents inputEvents;
        private readonly SetParkingBrakeEvent _event;

        internal void SetParkingBrake(ExtendedSimConnect sc, Boolean? isSet)
        {
            if (sc.IsAtr7x)
            {
                // The ATR doesn't respect the default events and also has a middle "emergency" position.
                // Luckily, the WinCntl PAC32 recognises this intermediate position, though you have to hold the control there.
                inputEvents.Send(sc, "HANDLING_LANDING_GEAR_SWITCH_PARKINGBRAKE", isSet switch { true => 2.0, false => 0.0, _ => 1.0 });
            }
            else if (isSet !=  null)
                sc.SendEvent(_event, isSet.Value ? 1 : 0);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBrakeSet : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly ParkingBreakRouter router;
        public int GetButton() => UrsaMinorThrottle.BUTTON_PARKING_BRAKE_SET;
        public void OnPress(ExtendedSimConnect sc) => router.SetParkingBrake(sc, true);
        public void OnRelease(ExtendedSimConnect sc) => router.SetParkingBrake(sc, null);
    }

    [Component, RequiredArgsConstructor]
    public partial class ParkingBrakeRelease : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly ParkingBreakRouter router;
        public int GetButton() => UrsaMinorThrottle.BUTTON_PARKING_BRAKE_RELEASED;
        public void OnPress(ExtendedSimConnect sc) => router.SetParkingBrake(sc, false);
        public void OnRelease(ExtendedSimConnect sc) => router.SetParkingBrake(sc, null);
    }
}
