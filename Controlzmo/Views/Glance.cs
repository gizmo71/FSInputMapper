using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using Windows.Gaming.Input;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Glance : ISwitchCallback<T16000mStick>, IAxisCallback<T16000mHotas>
    {
        private readonly ILogger<Glance> _log;
        private readonly ResetView resetView;
        private readonly TaxiCam taxiCam;
        private readonly VirtualJoy vJoy;

        public int GetSwitch() => T16000mStick.SWITCH_TOP_HAT;

        private GameControllerSwitchPosition current = GameControllerSwitchPosition.Center;

        public void OnChange(ExtendedSimConnect simConnect, GameControllerSwitchPosition old, GameControllerSwitchPosition @new)
        {
            _log.LogDebug($"top hat {old}/{current}->{@new}");
            switch (@new)
            {
                case GameControllerSwitchPosition.UpLeft:
                case GameControllerSwitchPosition.UpRight:
                case GameControllerSwitchPosition.DownLeft:
                case GameControllerSwitchPosition.DownRight:
                    @new = current;
                    break;
            }
            if (@new == current)
                return;
            switch (@new)
            {
                case GameControllerSwitchPosition.Left: vJoy.getController().QuickClick(104u); break;
                case GameControllerSwitchPosition.Right: vJoy.getController().QuickClick(106u); break;
                case GameControllerSwitchPosition.Up:
                    simConnect.SendDataOnSimObject(new CameraVariableData() { cameraState = 3, cameraSubState = 3, viewType = 4, viewIndex = 3 });
                    break;
                case GameControllerSwitchPosition.Down:
                    simConnect.RequestDataOnSimObject(taxiCam, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                    break;
                default:
                    simConnect.SendDataOnSimObject(new ResetViewData() { cameraState = 2 });
                    resetView.OnPress(simConnect);
                    break;
            }
            current = @new;
        }

        public int GetAxis() => T16000mHotas.AXIS_WHEEL;

        public void OnChange(ExtendedSimConnect simConnect, double old, double @new)
        {
            simConnect.CameraSetRelative6DOF(0f, 100f, -15f, 180f * (float) @new, 0f, 0f);
        }
    }
}
